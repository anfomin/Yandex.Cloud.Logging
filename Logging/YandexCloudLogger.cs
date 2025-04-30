using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace Yandex.Cloud.Logging;

/// <summary>
/// Creates log entries for a category and sends them to the Yandex.Cloud via <paramref name="service"/>.
/// </summary>
public sealed class YandexCloudLogger(
	string categoryName,
	YandexCloudLoggerService service,
	IEnumerable<IYandexCloudLogEntryProvider> entryProviders) : ILogger
{
	readonly string _categoryName = categoryName;
	readonly YandexCloudLoggerService _service = service;
	readonly IEnumerable<IYandexCloudLogEntryProvider> _entryProviders = entryProviders;

	/// <inheritdoc />
	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		=> null;

	/// <inheritdoc />
	public bool IsEnabled(LogLevel logLevel)
		=> logLevel != LogLevel.None;

	/// <inheritdoc />
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
			return;

		V1.IncomingLogEntry entry = new()
		{
			Timestamp = DateTime.UtcNow.ToTimestamp(),
			Level = logLevel.ToYandex(),
			StreamName = _categoryName,
			Message = formatter(state, exception)
		};
		Struct? payload = null;
		foreach (var provider in _entryProviders)
		{
			payload ??= new();
			provider.ApplyPayload(payload);
		}
		if (exception != null)
		{
			payload ??= new();
			ApplyPayloadException(payload, exception);
		}
		if (payload?.Fields.Count > 0)
			entry.JsonPayload = payload;
		_service.EnqueueLog(entry);
	}

	static void ApplyPayloadException(Struct payload, Exception exception)
	{
		Exception? ex = exception;
		List<Value> exValues = [];
		while (ex != null)
		{
			Struct exPayload = new()
			{
				Fields =
				{
					["type"] = Value.ForString(ex.GetType().FullName),
					["message"] = Value.ForString(ex.Message)
				}
			};
			if (ex.StackTrace is {} stackTrace)
				exPayload.Fields["stack_trace"] = Value.ForList(stackTrace
					.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
					.Select(Value.ForString)
					.ToArray()
				);
			exValues.Add(Value.ForStruct(exPayload));
			ex = ex.InnerException;
		}
		payload.Fields["exceptions"] = Value.ForList(exValues.ToArray());
	}
}