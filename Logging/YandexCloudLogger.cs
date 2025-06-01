using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Type = System.Type;

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

		if (exception == null)
			LogInternal(logLevel, state, null, formatter);
		else
		{
			var options = _service.Options;
			foreach (var ex in StripWrapperExceptions(exception, options.WrapperExceptions))
			{
				if (!options.IgnoreCanceledExceptions || (ex is not OperationCanceledException && ex.GetBaseException() is not OperationCanceledException))
					LogInternal(logLevel, state, ex, formatter);
			}
		}
	}

	void LogInternal<TState>(LogLevel logLevel, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
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

	/// <summary>
	/// Returns inner exceptions if <paramref name="exception"/> is any of <paramref name="wrapperExceptionTypes"/>.
	/// </summary>
	/// <param name="wrapperExceptionTypes">Exception types to strip.</param>
	static IEnumerable<Exception> StripWrapperExceptions(Exception exception, IEnumerable<Type> wrapperExceptionTypes)
	{
		if (exception.InnerException != null && wrapperExceptionTypes.Contains(exception.GetType()))
		{
			if (exception is AggregateException ae)
			{
				foreach (var inner in ae.InnerExceptions)
				foreach (var ex in StripWrapperExceptions(inner, wrapperExceptionTypes))
					yield return ex;
			}
			else
			{
				foreach (var ex in StripWrapperExceptions(exception.InnerException, wrapperExceptionTypes))
					yield return ex;
			}
		}
		else
		{
			yield return exception;
		}
	}
}