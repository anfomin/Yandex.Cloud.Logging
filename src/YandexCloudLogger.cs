using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Yandex.Cloud.Logging.V1;
using static Yandex.Cloud.Logging.V1.LogLevel.Types;
using LoggerLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Yandex.Cloud.Logging;

/// <summary>
/// Creates log entries for a category and sends them to Yandex.Cloud via <paramref name="service"/>.
/// </summary>
public sealed class YandexCloudLogger(string categoryName, YandexCloudLoggerService service) : ILogger
{
	readonly string _categoryName = categoryName;
	readonly YandexCloudLoggerService _service = service;

	/// <inheritdoc />
	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		=> null;

	/// <inheritdoc />
	public bool IsEnabled(LoggerLevel logLevel)
		=> logLevel != LoggerLevel.None;

	/// <inheritdoc />
	public void Log<TState>(LoggerLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
			return;

		var payload = new Struct();
		payload.Fields["category"] = Value.ForString(_categoryName);
		if (exception != null)
		{
			List<Value> exceptionValues = [];
			while (exception != null)
			{
				var exceptionPayload = new Struct();
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
			payload.Fields["exceptions"] = Value.ForList(exceptionValues.ToArray());
		}

		IncomingLogEntry entry = new()
		{
			Timestamp = DateTime.UtcNow.ToTimestamp(),
			Level = GetYandexLogLevel(logLevel),
			Message = formatter(state, exception),
			JsonPayload = payload
		};
		_service.EnqueueLog(entry);
	}

	static Level GetYandexLogLevel(LoggerLevel logLevel) => logLevel switch
	{
		LoggerLevel.Trace => Level.Trace,
		LoggerLevel.Debug => Level.Debug,
		LoggerLevel.Information => Level.Info,
		LoggerLevel.Warning => Level.Warn,
		LoggerLevel.Error => Level.Error,
		LoggerLevel.Critical => Level.Fatal,
		_ => Level.Unspecified
	};
}