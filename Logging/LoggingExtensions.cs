using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;
using Yandex.Cloud.Logging;

namespace Microsoft.Extensions.Logging;

/// <summary>
/// <see cref="ILoggingBuilder"/> extension methods for the Yandex.Cloud logging registration.
/// </summary>
public static class YandexCloudLoggingExtensions
{
	/// <summary>
	/// Adds a Yandex.Cloud logger to the factory and registers required services.
	/// </summary>
	public static ILoggingBuilder AddYandexCloud(this ILoggingBuilder builder)
	{
		builder.AddConfiguration();
		builder.Services.TryAddSingleton<YandexCloudLoggerService>();
		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, YandexCloudLoggerProvider>());
		builder.Services.AddHostedService(s => s.GetRequiredService<YandexCloudLoggerService>());
		LoggerProviderOptions.RegisterProviderOptions<YandexCloudLoggerOptions, YandexCloudLoggerProvider>(builder.Services);
		return builder;
	}

	/// <summary>
	/// Adds a Yandex.Cloud logger to the factory and registers required services.
	/// </summary>
	/// <param name="configure">A delegate to configure the <see cref="YandexCloudLoggerOptions"/>.</param>
	public static ILoggingBuilder AddYandexCloud(this ILoggingBuilder builder, Action<YandexCloudLoggerOptions> configure)
	{
		builder.AddYandexCloud();
		builder.Services.Configure(configure);
		return builder;
	}

	/// <summary>
	/// Registers custom <see cref="IYandexCloudLogEntryProvider"/> to provide additional payload to the Yandex.Cloud logger.
	/// </summary>
	/// <typeparam name="TProvider">Custom provider type.</typeparam>
	public static ILoggingBuilder AddYandexCloudLogEntryProvider<TProvider>(this ILoggingBuilder builder)
		where TProvider : class, IYandexCloudLogEntryProvider
	{
		builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IYandexCloudLogEntryProvider, TProvider>());
		return builder;
	}

	/// <summary>
	/// Writes a log entry with additional details object.
	/// </summary>
	/// <param name="logLevel">Entry will be written on this level.</param>
	/// <param name="eventId">ID of the event.</param>
	/// <param name="details">Additional details object.</param>
	/// <param name="exception">The exception related to this entry.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public static void LogDetails(this ILogger logger, LogLevel logLevel, EventId eventId, object details, Exception? exception,
		[StructuredMessageTemplate] string? message, params object?[] args)
	{
		var formatted = FormattedLogValuesHelper.Create(message, args);
		LogState state = new(formatted) { Details = details };
		logger.Log(logLevel, eventId, state, exception, (s, _) => s.ToString());
	}

	/// <summary>
	/// Writes a debug log entry with additional details object.
	/// </summary>
	/// <param name="eventId">ID of the event.</param>
	/// <param name="details">Additional details object.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public static void LogDebugDetails(this ILogger logger, EventId eventId, object details, [StructuredMessageTemplate] string? message, params object?[] args)
		=> logger.LogDetails(LogLevel.Debug, eventId, details, null, message, args);

	/// <summary>
	/// Writes a debug log entry with additional details object.
	/// </summary>
	/// <param name="details">Additional details object.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public static void LogDebugDetails(this ILogger logger, object details, [StructuredMessageTemplate] string? message, params object?[] args)
		=> logger.LogDetails(LogLevel.Debug, 0, details, null, message, args);

	/// <summary>
	/// Writes an information log entry with additional details object.
	/// </summary>
	/// <param name="eventId">ID of the event.</param>
	/// <param name="details">Additional details object.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public static void LogInformationDetails(this ILogger logger, EventId eventId, object details, [StructuredMessageTemplate] string? message, params object?[] args)
		=> logger.LogDetails(LogLevel.Information, eventId, details, null, message, args);

	/// <summary>
	/// Writes an information log entry with additional details object.
	/// </summary>
	/// <param name="details">Additional details object.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public static void LogInformationDetails(this ILogger logger, object details, [StructuredMessageTemplate] string? message, params object?[] args)
		=> logger.LogDetails(LogLevel.Information, 0, details, null, message, args);

	/// <summary>
	/// Writes a warning log entry with additional details object.
	/// </summary>
	/// <param name="eventId">ID of the event.</param>
	/// <param name="details">Additional details object.</param>
	/// <param name="exception">The exception related to this entry.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public static void LogWarningDetails(this ILogger logger, EventId eventId, object details, Exception exception, [StructuredMessageTemplate] string? message, params object?[] args)
		=> logger.LogDetails(LogLevel.Warning, eventId, details, exception, message, args);

	/// <summary>
	/// Writes a warning log entry with additional details object.
	/// </summary>
	/// <param name="eventId">ID of the event.</param>
	/// <param name="details">Additional details object.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public static void LogWarningDetails(this ILogger logger, EventId eventId, object details, [StructuredMessageTemplate] string? message, params object?[] args)
		=> logger.LogDetails(LogLevel.Warning, eventId, details, null, message, args);

	/// <summary>
	/// Writes a warning log entry with additional details object.
	/// </summary>
	/// <param name="details">Additional details object.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public static void LogWarningDetails(this ILogger logger, object details, [StructuredMessageTemplate] string? message, params object?[] args)
		=> logger.LogDetails(LogLevel.Warning, 0, details, null, message, args);

	/// <summary>
	/// Writes an error log entry with additional details object.
	/// </summary>
	/// <param name="eventId">ID of the event.</param>
	/// <param name="details">Additional details object.</param>
	/// <param name="exception">The exception related to this entry.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public static void LogErrorDetails(this ILogger logger, EventId eventId, object details, Exception exception, [StructuredMessageTemplate] string? message, params object?[] args)
		=> logger.LogDetails(LogLevel.Error, eventId, details, exception, message, args);

	/// <summary>
	/// Writes an error log entry with additional details object.
	/// </summary>
	/// <param name="eventId">ID of the event.</param>
	/// <param name="details">Additional details object.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public static void LogErrorDetails(this ILogger logger, EventId eventId, object details, [StructuredMessageTemplate] string? message, params object?[] args)
		=> logger.LogDetails(LogLevel.Error, eventId, details, null, message, args);

	/// <summary>
	/// Writes an error log entry with additional details object.
	/// </summary>
	/// <param name="details">Additional details object.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public static void LogErrorDetails(this ILogger logger, object details, [StructuredMessageTemplate] string? message, params object?[] args)
		=> logger.LogDetails(LogLevel.Error, 0, details, null, message, args);

	/// <summary>
	/// Writes a critical log entry with additional details object.
	/// </summary>
	/// <param name="eventId">ID of the event.</param>
	/// <param name="details">Additional details object.</param>
	/// <param name="exception">The exception related to this entry.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public static void LogCriticalDetails(this ILogger logger, EventId eventId, object details, Exception exception, [StructuredMessageTemplate] string? message, params object?[] args)
		=> logger.LogDetails(LogLevel.Critical, eventId, details, exception, message, args);

	/// <summary>
	/// Writes a critical log entry with additional details object.
	/// </summary>
	/// <param name="eventId">ID of the event.</param>
	/// <param name="details">Additional details object.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public static void LogCriticalDetails(this ILogger logger, EventId eventId, object details, [StructuredMessageTemplate] string? message, params object?[] args)
		=> logger.LogDetails(LogLevel.Critical, eventId, details, null, message, args);

	/// <summary>
	/// Writes a critical log entry with additional details object.
	/// </summary>
	/// <param name="details">Additional details object.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public static void LogCriticalDetails(this ILogger logger, object details, [StructuredMessageTemplate] string? message, params object?[] args)
		=> logger.LogDetails(LogLevel.Critical, 0, details, null, message, args);
}