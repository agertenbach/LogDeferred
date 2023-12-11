using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using static Microsoft.Extensions.DependencyInjection.DeferredLoggerExtensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for generating deferred logs during application build and startup
    /// </summary>
    public static class DeferredLoggerWebApplicationBuilderExtensions
    {
        #region Action loggers

        /// <summary>
        /// Customize the name of the default logger used for deferred startup logs
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="name">A non-null and non-empty string</param>
        public static void SetStartupLoggerName(this WebApplicationBuilder builder, string name)
        {
            builder.Services.SetStartupLoggerName(name);
        }

        /// <summary>
        /// Log against an <see cref="ILogger"/> instance deferred until after startup
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static DeferredLoggerBuilder Log(this WebApplicationBuilder builder)
        {
            return new DeferredLoggerBuilder(builder.Services);
        }

        /// <summary>
        /// Log against an <see cref="ILogger"/> instance for type <typeparamref name="TLogger"/> deferred until after startup
        /// </summary>
        /// <typeparam name="TLogger"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static DeferredLoggerBuilder Log<TLogger>(this WebApplicationBuilder builder)
        {
            return new DeferredLoggerBuilder<TLogger>(builder.Services);
        }

        /// <summary>
        /// Log against an <see cref="ILogger"/> instance deferred until after startup
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="action">Logger action</param>
        /// <returns></returns>
        public static void Log(this WebApplicationBuilder builder, Action<ILogger> action)
        {
            builder.Services.DeferLog(action);
        }

        /// <summary>
        /// Log against an <see cref="ILogger"/> instance for type <typeparamref name="TLogger"/> deferred until after startup
        /// </summary>
        /// <typeparam name="TLogger">Logger category</typeparam>
        /// <param name="builder"></param>
        /// <param name="action">Logger action</param>
        /// <returns></returns>
        public static void Log<TLogger>(this WebApplicationBuilder builder, Action<ILogger> action)
        {
            builder.Services.DeferLog<TLogger>(action);
        }

        #endregion

        #region Extensions - ILogger Generic
        #pragma warning disable CA2254 // Template should be a static expression
        //------------------------------------------DEBUG------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogDebug(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogDebug(this WebApplicationBuilder builder, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Debug, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogDebug(0, "Processing request from {Address}", address)</example>
        public static void LogDebug(this WebApplicationBuilder builder, EventId eventId, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Debug, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogDebug(exception, "Error while processing request from {Address}", address)</example>
        public static void LogDebug(this WebApplicationBuilder builder, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Debug, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogDebug("Processing request from {Address}", address)</example>
        public static void LogDebug(this WebApplicationBuilder builder, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Debug, message, args));
        }

        //------------------------------------------TRACE------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogTrace(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogTrace(this WebApplicationBuilder builder, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Trace, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogTrace(0, "Processing request from {Address}", address)</example>
        public static void LogTrace(this WebApplicationBuilder builder, EventId eventId, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Trace, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogTrace(exception, "Error while processing request from {Address}", address)</example>
        public static void LogTrace(this WebApplicationBuilder builder, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Trace, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogTrace("Processing request from {Address}", address)</example>
        public static void LogTrace(this WebApplicationBuilder builder, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Trace, message, args));
        }

        //------------------------------------------INFORMATION------------------------------------------//

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogInformation(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogInformation(this WebApplicationBuilder builder, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Information, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogInformation(0, "Processing request from {Address}", address)</example>
        public static void LogInformation(this WebApplicationBuilder builder, EventId eventId, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Information, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogInformation(exception, "Error while processing request from {Address}", address)</example>
        public static void LogInformation(this WebApplicationBuilder builder, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Information, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogInformation("Processing request from {Address}", address)</example>
        public static void LogInformation(this WebApplicationBuilder builder, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Information, message, args));
        }

        //------------------------------------------WARNING------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogWarning(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogWarning(this WebApplicationBuilder builder, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Warning, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogWarning(0, "Processing request from {Address}", address)</example>
        public static void LogWarning(this WebApplicationBuilder builder, EventId eventId, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Warning, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogWarning(exception, "Error while processing request from {Address}", address)</example>
        public static void LogWarning(this WebApplicationBuilder builder, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Warning, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogWarning("Processing request from {Address}", address)</example>
        public static void LogWarning(this WebApplicationBuilder builder, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Warning, message, args));
        }

        //------------------------------------------ERROR------------------------------------------//

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogError(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogError(this WebApplicationBuilder builder, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Error, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogError(0, "Processing request from {Address}", address)</example>
        public static void LogError(this WebApplicationBuilder builder, EventId eventId, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Error, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogError(exception, "Error while processing request from {Address}", address)</example>
        public static void LogError(this WebApplicationBuilder builder, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Error, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogError("Processing request from {Address}", address)</example>
        public static void LogError(this WebApplicationBuilder builder, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Error, message, args));
        }

        //------------------------------------------CRITICAL------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogCritical(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogCritical(this WebApplicationBuilder builder, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Critical, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogCritical(0, "Processing request from {Address}", address)</example>
        public static void LogCritical(this WebApplicationBuilder builder, EventId eventId, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Critical, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogCritical(exception, "Error while processing request from {Address}", address)</example>
        public static void LogCritical(this WebApplicationBuilder builder, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Critical, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogCritical("Processing request from {Address}", address)</example>
        public static void LogCritical(this WebApplicationBuilder builder, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(LogLevel.Critical, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log(this WebApplicationBuilder builder, LogLevel logLevel, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(logLevel, 0, null, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log(this WebApplicationBuilder builder, LogLevel logLevel, EventId eventId, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(logLevel, eventId, null, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log(this WebApplicationBuilder builder, LogLevel logLevel, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(logLevel, 0, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log(this WebApplicationBuilder builder, LogLevel logLevel, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog(logger => logger.Log(logLevel, eventId, exception, message, args));
        }
        #endregion

        #region Extensions - ILogger of T

        //------------------------------------------DEBUG------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogDebug(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogDebug<TLogger>(this WebApplicationBuilder builder, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Debug, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogDebug(0, "Processing request from {Address}", address)</example>
        public static void LogDebug<TLogger>(this WebApplicationBuilder builder, EventId eventId, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Debug, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogDebug(exception, "Error while processing request from {Address}", address)</example>
        public static void LogDebug<TLogger>(this WebApplicationBuilder builder, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Debug, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogDebug("Processing request from {Address}", address)</example>
        public static void LogDebug<TLogger>(this WebApplicationBuilder builder, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Debug, message, args));
        }

        //------------------------------------------TRACE------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary>
        /// <typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogTrace(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogTrace<TLogger>(this WebApplicationBuilder builder, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Trace, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary>
        /// <typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogTrace(0, "Processing request from {Address}", address)</example>
        public static void LogTrace<TLogger>(this WebApplicationBuilder builder, EventId eventId, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Trace, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary>
        /// <typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogTrace(exception, "Error while processing request from {Address}", address)</example>
        public static void LogTrace<TLogger>(this WebApplicationBuilder builder, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Trace, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogTrace("Processing request from {Address}", address)</example>
        public static void LogTrace<TLogger>(this WebApplicationBuilder builder, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Trace, message, args));
        }

        //------------------------------------------INFORMATION------------------------------------------//

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogInformation(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogInformation<TLogger>(this WebApplicationBuilder builder, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Information, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogInformation(0, "Processing request from {Address}", address)</example>
        public static void LogInformation<TLogger>(this WebApplicationBuilder builder, EventId eventId, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Information, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogInformation(exception, "Error while processing request from {Address}", address)</example>
        public static void LogInformation<TLogger>(this WebApplicationBuilder builder, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Information, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogInformation("Processing request from {Address}", address)</example>
        public static void LogInformation<TLogger>(this WebApplicationBuilder builder, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Information, message, args));
        }

        //------------------------------------------WARNING------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogWarning(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogWarning<TLogger>(this WebApplicationBuilder builder, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Warning, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogWarning(0, "Processing request from {Address}", address)</example>
        public static void LogWarning<TLogger>(this WebApplicationBuilder builder, EventId eventId, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Warning, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogWarning(exception, "Error while processing request from {Address}", address)</example>
        public static void LogWarning<TLogger>(this WebApplicationBuilder builder, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Warning, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogWarning("Processing request from {Address}", address)</example>
        public static void LogWarning<TLogger>(this WebApplicationBuilder builder, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Warning, message, args));
        }

        //------------------------------------------ERROR------------------------------------------//

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogError(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogError<TLogger>(this WebApplicationBuilder builder, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Error, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogError(0, "Processing request from {Address}", address)</example>
        public static void LogError<TLogger>(this WebApplicationBuilder builder, EventId eventId, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Error, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogError(exception, "Error while processing request from {Address}", address)</example>
        public static void LogError<TLogger>(this WebApplicationBuilder builder, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Error, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogError("Processing request from {Address}", address)</example>
        public static void LogError<TLogger>(this WebApplicationBuilder builder, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Error, message, args));
        }

        //------------------------------------------CRITICAL------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogCritical(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogCritical<TLogger>(this WebApplicationBuilder builder, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Critical, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogCritical(0, "Processing request from {Address}", address)</example>
        public static void LogCritical<TLogger>(this WebApplicationBuilder builder, EventId eventId, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Critical, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogCritical(exception, "Error while processing request from {Address}", address)</example>
        public static void LogCritical<TLogger>(this WebApplicationBuilder builder, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Critical, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>builder.LogCritical("Processing request from {Address}", address)</example>
        public static void LogCritical<TLogger>(this WebApplicationBuilder builder, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Critical, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log<TLogger>(this WebApplicationBuilder builder, LogLevel logLevel, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(logLevel, 0, null, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log<TLogger>(this WebApplicationBuilder builder, LogLevel logLevel, EventId eventId, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(logLevel, eventId, null, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log<TLogger>(this WebApplicationBuilder builder, LogLevel logLevel, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(logLevel, 0, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log<TLogger>(this WebApplicationBuilder builder, LogLevel logLevel, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            builder.Services.DeferLog<TLogger>(logger => logger.Log(logLevel, eventId, exception, message, args));
        }
        #endregion

        #pragma warning restore CA2254 // Template should be a static expression

    }
}
