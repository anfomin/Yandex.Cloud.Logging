using JetBrains.Annotations;
using Yandex.Cloud.Logging;

namespace Microsoft.Extensions.Logging;

public static partial class YandexCloudLoggingExtensions
{
	extension(ILogger logger)
	{
		/// <summary>
		/// Writes a log entry with additional details object.
		/// </summary>
		/// <param name="logLevel">Entry will be written on this level.</param>
		/// <param name="eventId">ID of the event.</param>
		/// <param name="details">Additional details object.</param>
		/// <param name="exception">The exception related to this entry.</param>
		/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		public void LogDetails(LogLevel logLevel, EventId eventId, object details, Exception? exception,
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
		public void LogDebugDetails(EventId eventId, object details, [StructuredMessageTemplate] string? message, params object?[] args)
			=> logger.LogDetails(LogLevel.Debug, eventId, details, null, message, args);

		/// <summary>
		/// Writes a debug log entry with additional details object.
		/// </summary>
		/// <param name="details">Additional details object.</param>
		/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		public void LogDebugDetails(object details, [StructuredMessageTemplate] string? message, params object?[] args)
			=> logger.LogDetails(LogLevel.Debug, 0, details, null, message, args);

		/// <summary>
		/// Writes an information log entry with additional details object.
		/// </summary>
		/// <param name="eventId">ID of the event.</param>
		/// <param name="details">Additional details object.</param>
		/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		public void LogInformationDetails(EventId eventId, object details, [StructuredMessageTemplate] string? message, params object?[] args)
			=> logger.LogDetails(LogLevel.Information, eventId, details, null, message, args);

		/// <summary>
		/// Writes an information log entry with additional details object.
		/// </summary>
		/// <param name="details">Additional details object.</param>
		/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		public void LogInformationDetails(object details, [StructuredMessageTemplate] string? message, params object?[] args)
			=> logger.LogDetails(LogLevel.Information, 0, details, null, message, args);

		/// <summary>
		/// Writes a warning log entry with additional details object.
		/// </summary>
		/// <param name="eventId">ID of the event.</param>
		/// <param name="details">Additional details object.</param>
		/// <param name="exception">The exception related to this entry.</param>
		/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		public void LogWarningDetails(EventId eventId, object details, Exception exception, [StructuredMessageTemplate] string? message, params object?[] args)
			=> logger.LogDetails(LogLevel.Warning, eventId, details, exception, message, args);

		/// <summary>
		/// Writes a warning log entry with additional details object.
		/// </summary>
		/// <param name="eventId">ID of the event.</param>
		/// <param name="details">Additional details object.</param>
		/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		public void LogWarningDetails(EventId eventId, object details, [StructuredMessageTemplate] string? message, params object?[] args)
			=> logger.LogDetails(LogLevel.Warning, eventId, details, null, message, args);

		/// <summary>
		/// Writes a warning log entry with additional details object.
		/// </summary>
		/// <param name="details">Additional details object.</param>
		/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		public void LogWarningDetails(object details, [StructuredMessageTemplate] string? message, params object?[] args)
			=> logger.LogDetails(LogLevel.Warning, 0, details, null, message, args);

		/// <summary>
		/// Writes an error log entry with additional details object.
		/// </summary>
		/// <param name="eventId">ID of the event.</param>
		/// <param name="details">Additional details object.</param>
		/// <param name="exception">The exception related to this entry.</param>
		/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		public void LogErrorDetails(EventId eventId, object details, Exception exception, [StructuredMessageTemplate] string? message, params object?[] args)
			=> logger.LogDetails(LogLevel.Error, eventId, details, exception, message, args);

		/// <summary>
		/// Writes an error log entry with additional details object.
		/// </summary>
		/// <param name="eventId">ID of the event.</param>
		/// <param name="details">Additional details object.</param>
		/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		public void LogErrorDetails(EventId eventId, object details, [StructuredMessageTemplate] string? message, params object?[] args)
			=> logger.LogDetails(LogLevel.Error, eventId, details, null, message, args);

		/// <summary>
		/// Writes an error log entry with additional details object.
		/// </summary>
		/// <param name="details">Additional details object.</param>
		/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		public void LogErrorDetails(object details, [StructuredMessageTemplate] string? message, params object?[] args)
			=> logger.LogDetails(LogLevel.Error, 0, details, null, message, args);

		/// <summary>
		/// Writes a critical log entry with additional details object.
		/// </summary>
		/// <param name="eventId">ID of the event.</param>
		/// <param name="details">Additional details object.</param>
		/// <param name="exception">The exception related to this entry.</param>
		/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		public void LogCriticalDetails(EventId eventId, object details, Exception exception, [StructuredMessageTemplate] string? message, params object?[] args)
			=> logger.LogDetails(LogLevel.Critical, eventId, details, exception, message, args);

		/// <summary>
		/// Writes a critical log entry with additional details object.
		/// </summary>
		/// <param name="eventId">ID of the event.</param>
		/// <param name="details">Additional details object.</param>
		/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		public void LogCriticalDetails(EventId eventId, object details, [StructuredMessageTemplate] string? message, params object?[] args)
			=> logger.LogDetails(LogLevel.Critical, eventId, details, null, message, args);

		/// <summary>
		/// Writes a critical log entry with additional details object.
		/// </summary>
		/// <param name="details">Additional details object.</param>
		/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		public void LogCriticalDetails(object details, [StructuredMessageTemplate] string? message, params object?[] args)
			=> logger.LogDetails(LogLevel.Critical, 0, details, null, message, args);
	}
}