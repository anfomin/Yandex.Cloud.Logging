
using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Yandex.Cloud.Logging.V1;

namespace Yandex.Cloud.Logging;

/// <summary>
/// Sends log entries to Yandex.Cloud in background.
/// </summary>
public class YandexCloudLoggerService : BackgroundService
{
	const int RetryCount = 10;
	static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

	YandexCloudLoggerOptions _options;
	readonly IDisposable? _optionsChangeToken;
	readonly Sdk? _sdkDefault;
	Sdk _sdk;
	readonly ConcurrentQueue<IncomingLogEntry> _queue = new();
	readonly SemaphoreSlim _queueSignal = new(0, 1);

	public YandexCloudLoggerService(IOptionsMonitor<YandexCloudLoggerOptions> options, Sdk? sdk = null)
	{
		_options = options.CurrentValue;
		_options.Validate();
		_optionsChangeToken = options.OnChange(opt =>
		{
			opt.Validate();
			_options = opt;
			_sdk = GetSdk();
		});
		_sdkDefault = sdk;
		_sdk = GetSdk();
	}

	Sdk GetSdk()
		=> _options.CredentialsProvider is {} cred
		? new Sdk(cred)
		: _sdkDefault
		?? throw new InvalidOperationException("Yandex.Cloud credentials provider must be set or SDK service registered.");

	/// <inheritdoc />
	public override void Dispose()
	{
		_optionsChangeToken?.Dispose();
		base.Dispose();
	}

	/// <summary>
	/// Enqueues a log entry to be sent to Yandex.Cloud.
	/// </summary>
	/// <param name="entry">Log entry to send.</param>
	public void EnqueueLog(IncomingLogEntry entry)
	{
		_queue.Enqueue(entry);
		try
		{
			_queueSignal.Release();
		}
		catch (SemaphoreFullException) { }
	}

	/// <inheritdoc />
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var sent = DateTime.MinValue;
		while (!stoppingToken.IsCancellationRequested)
		{
			if (_queue.Count < _options.BatchSize)
			{
				if (_queue.IsEmpty)
					await _queueSignal.WaitAsync(stoppingToken);

				var passed = DateTime.UtcNow - sent;
				if (passed < _options.CacheTime && await _queueSignal.WaitAsync(_options.CacheTime - passed, stoppingToken))
					continue;
			}

			List<IncomingLogEntry> entries = [];
			for (int i = 0; i < _options.BatchSize; i++)
			{
				if (!_queue.TryDequeue(out var entry))
					break;
				if (_options.StreamName != null && string.IsNullOrEmpty(entry.StreamName))
					entry.StreamName = _options.StreamName;
				entries.Add(entry);
			}

			try
			{
				await SendLogsAsync(entries, stoppingToken);
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				Debug.WriteLine(ex.Message);
			}
			sent = DateTime.UtcNow;
		}
	}

	async Task SendLogsAsync(List<IncomingLogEntry> entries, CancellationToken cancellationToken = default)
	{
		int retry = 0;
		while (true)
		{
			retry++;
			try
			{
				WriteRequest request = new()
				{
					Destination = new()
					{
						FolderId = _options.FolderId,
						LogGroupId = _options.GroupId
					},
					Resource = new()
					{
						Id = _options.ResourceId ?? "",
						Type = _options.ResourceType ?? ""
					},
				};
				request.Entries.Add(entries);
				await _sdk.Services.Logging.LogIngestionService.WriteAsync(request, cancellationToken: cancellationToken);
				return;
			}
			catch (Exception ex) when (ex is not OperationCanceledException && retry <= RetryCount)
			{
				await Task.Delay(RetryDelay, cancellationToken);
			}
		}
	}
}