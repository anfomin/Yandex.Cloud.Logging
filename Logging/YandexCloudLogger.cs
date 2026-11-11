using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Yandex.Cloud.Logging.V1;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Type = System.Type;

namespace Yandex.Cloud.Logging;

/// <summary>
/// Creates log entries for a category and sends them to the Yandex.Cloud via <paramref name="service"/>.
/// </summary>
public sealed class YandexCloudLogger(
	string categoryName,
	YandexCloudLoggerService service,
	IEnumerable<IYandexCloudLogEntryProvider> entryProviders)
	: ILogger
{
	readonly string _categoryName = categoryName;
	readonly YandexCloudLoggerService _service = service;
	readonly IEnumerable<IYandexCloudLogEntryProvider> _entryProviders = entryProviders;

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		=> null;

	public bool IsEnabled(LogLevel logLevel)
		=> logLevel != LogLevel.None;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
			return;

		if (exception == null)
			LogInternal(logLevel, eventId, state, null, formatter);
		else
		{
			var options = _service.Options;
			foreach (var ex in StripWrapperExceptions(exception, options.WrapperExceptions))
			{
				if (!options.IgnoreCanceledExceptions || (ex is not OperationCanceledException && ex.GetBaseException() is not OperationCanceledException))
					LogInternal(logLevel, eventId, state, ex, formatter);
			}
		}
	}

	void LogInternal<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		IncomingLogEntry entry = new()
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
		if (eventId != 0)
		{
			payload ??= new();
			ApplyPayloadEventId(payload, eventId);
		}
		if (exception != null)
		{
			payload ??= new();
			ApplyPayloadException(payload, exception);
		}
		if (state is LogState { Details: { } details })
		{
			payload ??= new();
			ApplyPayloadDetails(payload, details);
		}
		if (payload?.Fields.Count > 0)
			entry.JsonPayload = payload;
		_service.EnqueueLog(entry);
	}

	static void ApplyPayloadEventId(Struct payload, EventId eventId)
	{
		Struct evt = new()
		{
			Fields = { ["id"] = Value.ForNumber(eventId.Id) }
		};
		if (eventId.Name is { } name)
			evt.Fields["name"] = Value.ForString(name);
		payload.Fields["event"] = Value.ForStruct(evt);
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
			if (ex.StackTrace is { } stackTrace)
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

	void ApplyPayloadDetails(Struct payload, object details)
	{
#if NET9_0_OR_GREATER
		var options = _service.Options.JsonSerializerOptions ?? JsonSerializerOptions.Web;
#else
		var options = _service.Options.JsonSerializerOptions ?? JsonSerializerOptions.Default;
#endif
		try
		{
			string json = JsonSerializer.Serialize(details, options);
			payload.Fields["details"] = Value.Parser.ParseJson(json);
		}
		catch (Exception ex)
		{
			payload.Fields["detailsException"] = Value.ForString(ex.Message);
		}
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