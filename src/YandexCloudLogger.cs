using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace Yandex.Cloud.Logging;

/// <summary>
/// Creates log entries for a category and sends them to the Yandex.Cloud via <paramref name="service"/>.
/// </summary>
public sealed class YandexCloudLogger(string categoryName, YandexCloudLoggerService service) : ILogger
{
	readonly string _categoryName = categoryName;
	readonly YandexCloudLoggerService _service = service;

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
		if (exception != null)
		{
			List<Value> exceptionValues = [];
			while (exception != null)
			{
				Struct exceptionPayload = new();
				exceptionPayload.Fields["type"] = Value.ForString(exception.GetType().FullName);
				exceptionPayload.Fields["message"] = Value.ForString(exception.Message);
				if (exception.StackTrace is {} stackTrace)
					exceptionPayload.Fields["stack_trace"] = Value.ForList(stackTrace
						.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
						.Select(line => Value.ForString(line))
						.ToArray()
					);
				exceptionValues.Add(Value.ForStruct(exceptionPayload));
				exception = exception.InnerException;
			}

			Struct payload = new();
			payload.Fields["exceptions"] = Value.ForList(exceptionValues.ToArray());
			entry.JsonPayload = payload;
		}
		_service.EnqueueLog(entry);
	}
}