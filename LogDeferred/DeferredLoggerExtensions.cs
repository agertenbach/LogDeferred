using DeferredLog.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LogDeferred.WebApplicationBuilder")]
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for generating deferred logs during application build and startup
    /// </summary>
    public static class DeferredLoggerExtensions
    {
        #region Internal
        /// <summary>
        /// Get or add a generic cached logger and invoke the logging action
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        internal static void DeferLog(this IServiceCollection services, Action<ILogger> action)
            => services.DeferLog(action, null);


        /// <summary>
        /// Run hosted service at startup to invoke logs
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        internal static IServiceCollection RunLogsAtStartup(this IServiceCollection services)
        {
            services.AddHostedService<StartupLoggerHostedService>();
            return services;
        }
        /// <summary>w
        /// Get or add a cached logger and invoke the logging action
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action">Logger action</param>
        /// <param name="loggerName">Category name for the logger</param>
        internal static void DeferLog(this IServiceCollection services, Action<ILogger> action, string? loggerName = null)
        {
            var builder = services.AddOptions<StartupLoggerCache>().Configure<ILoggerFactory>((q, f) =>
            {
                string name = loggerName ?? q.StartupLoggerName ?? StartupLoggerCache.DefaultStartupLoggerName;
                var logger = q.Loggers.GetOrAdd(name, f.CreateLogger(name));
                action(logger);
                q.HasLogged = true;
            });

            services.RunLogsAtStartup();
        }

        /// <summary>
        /// Get or add a cached logger for <typeparamref name="TLogger"/> and invoke the logging action
        /// </summary>
        /// <typeparam name="TLogger">Logger category by type</typeparam>
        /// <param name="services"></param>
        /// <param name="action">Logger action</param>
        internal static void DeferLog<TLogger>(this IServiceCollection services, Action<ILogger> action)
        {
            services.DeferLog(action, typeof(TLogger).FullName ?? typeof(TLogger).Name);
        }

#endregion

        #region Builder classes

        /// <summary>
        /// Builder for applying dependencies to a deferred logger
        /// </summary>
        public class DeferredLoggerBuilder 
        {
            internal string? TypeName { get; }

            internal IServiceCollection Services { get; }

            /// <summary>
            /// Builder for applying dependencies to a deferred logger
            /// </summary>
            /// <param name="services"></param>
            /// <param name="typeName"></param>
            public DeferredLoggerBuilder(IServiceCollection services, string? typeName = null)
            {
                Services = services;
                TypeName = typeName;
            }
        }

        /// <summary>
        /// Builder for applying dependencies to a deferred logger
        /// </summary>
        /// <typeparam name="TLogger"></typeparam>
        public class DeferredLoggerBuilder<TLogger> : DeferredLoggerBuilder
        {
            /// <summary>
            /// Builder for applying dependencies to a deferred logger
            /// </summary>
            /// <param name="services"></param>
            public DeferredLoggerBuilder(IServiceCollection services) : base(services, typeof(TLogger).FullName ?? typeof(TLogger).Name) { }
        }

        /// <summary>
        /// Caches logger instances during startup logging for re-use
        /// </summary>
        public class StartupLoggerCache
        {
            /// <summary>
            /// Default startup logger name
            /// </summary>
            public const string DefaultStartupLoggerName = "Microsoft.Hosting.Startup";

            /// <summary>
            /// The logger name that will be used for startup logs that are not assigned an explicit logger
            /// </summary>
            public string StartupLoggerName { get; set; } = DefaultStartupLoggerName;

            /// <summary>
            /// Cached loggers for re-use
            /// </summary>
            public ConcurrentDictionary<string, ILogger> Loggers { get; set; } = new ConcurrentDictionary<string, ILogger>();

            /// <summary>
            /// Have any startup logs been added?
            /// </summary>
            public bool HasLogged { get; set; }

            /// <summary>
            /// Safely get a cached logger for the supplied name or create and cache a new one
            /// </summary>
            /// <param name="factory"></param>
            /// <param name="name"></param>
            /// <returns></returns>
            public ILogger LoggerFor(ILoggerFactory factory, string? name = null)
            {
                string loggerName = name ?? StartupLoggerName ?? StartupLoggerCache.DefaultStartupLoggerName;
                var logger = Loggers.GetOrAdd(loggerName, factory.CreateLogger(loggerName));
                return logger;
            }
        }
        #endregion

        #region Action loggers
        
        /// <summary>
        /// Customize the name of the default logger used for deferred startup logs
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name">A non-null and non-empty string</param>
        public static void SetStartupLoggerName(this IServiceCollection services, string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                services.AddOptions<StartupLoggerCache>().Configure((q) =>
                {
                    q.StartupLoggerName = name;
                });
            }        
        }

        /// <summary>
        /// Log against an <see cref="ILogger"/> instance deferred until after startup
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static DeferredLoggerBuilder Log(this IServiceCollection services)
        {
            return new DeferredLoggerBuilder(services);
        }

        /// <summary>
        /// Log against an <see cref="ILogger"/> instance for type <typeparamref name="TLogger"/> deferred until after startup
        /// </summary>
        /// <typeparam name="TLogger"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static DeferredLoggerBuilder Log<TLogger>(this IServiceCollection services)
        {
            return new DeferredLoggerBuilder<TLogger>(services);
        }

        /// <summary>
        /// Log against an <see cref="ILogger"/> instance deferred until after startup
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action">Logger action</param>
        /// <returns></returns>
        public static void Log(this IServiceCollection services, Action<ILogger> action)
        {
            services.DeferLog(action);
        }

        /// <summary>
        /// Log against an <see cref="ILogger"/> instance for type <typeparamref name="TLogger"/> deferred until after startup
        /// </summary>
        /// <typeparam name="TLogger"></typeparam>
        /// <param name="services"></param>
        /// <param name="action">Logger action</param>
        /// <returns></returns>
        public static void Log<TLogger>(this IServiceCollection services, Action<ILogger> action)
        {
            services.DeferLog<TLogger>(action);
        }

        #endregion

        #region Builders - For Required Dependencies

        /// <summary>
        /// Access an ILogger using a required injected dependency of <typeparamref name="TDependency1"/>
        /// </summary>
        /// <typeparam name="TDependency1">Injected dependency</typeparam>
        /// <param name="b"></param>
        /// <param name="action">Logger action</param>
        /// <returns></returns>
        public static DeferredLoggerBuilder ForRequired<TDependency1>(this DeferredLoggerBuilder b, Action<ILogger, TDependency1> action)
            where TDependency1 : class
        {
            b.Services.AddOptions<StartupLoggerCache>().Configure<ILoggerFactory, TDependency1>((q, f, d1) =>
            {
                var logger = q.LoggerFor(f, b.TypeName);
                action(logger, d1);
            });
            b.Services.RunLogsAtStartup();

            return b;
        }

        /// <summary>
        /// Access an ILogger using a required injected dependency of <typeparamref name="TDependency1"/> and <typeparamref name="TDependency2"/>
        /// </summary>
        /// <remarks>
        /// If a required dependency is missing, an <see cref="InvalidOperationException"/> will be thrown
        /// </remarks>
        /// <typeparam name="TDependency1">Injected dependency</typeparam>
        /// <typeparam name="TDependency2">Injected dependency</typeparam>
        /// <param name="b"></param>
        /// <param name="action">Logger action</param>
        /// <returns></returns>
        public static DeferredLoggerBuilder ForRequired<TDependency1, TDependency2>(this DeferredLoggerBuilder b, Action<ILogger, TDependency1, TDependency2> action)
            where TDependency1 : class
            where TDependency2 : class
        {
            b.Services.AddOptions<StartupLoggerCache>().Configure<ILoggerFactory, TDependency1, TDependency2>((q, f, d1, d2) =>
            {
                var logger = q.LoggerFor(f, b.TypeName);
                action(logger, d1, d2);
            });
            b.Services.RunLogsAtStartup();

            return b;
        }

        /// <summary>
        /// Access an ILogger using a required injected dependency of <typeparamref name="TDependency1"/>, <typeparamref name="TDependency2"/>, and <typeparamref name="TDependency3"/>
        /// </summary>
        /// <remarks>
        /// If a required dependency is missing, an <see cref="InvalidOperationException"/> will be thrown
        /// </remarks>
        /// <typeparam name="TDependency1">Injected dependency</typeparam>
        /// <typeparam name="TDependency2">Injected dependency</typeparam>
        /// <typeparam name="TDependency3">Injected dependency</typeparam>
        /// <param name="b"></param>
        /// <param name="action">Logger action</param>
        /// <returns></returns>
        public static DeferredLoggerBuilder ForRequired<TDependency1, TDependency2, TDependency3>(this DeferredLoggerBuilder b, Action<ILogger, TDependency1, TDependency2, TDependency3> action)
            where TDependency1 : class
            where TDependency2 : class
            where TDependency3 : class
        {
            b.Services.AddOptions<StartupLoggerCache>().Configure<ILoggerFactory, TDependency1, TDependency2, TDependency3>((q, f, d1, d2, d3) =>
            {
                var logger = q.LoggerFor(f, b.TypeName);
                action(logger, d1, d2, d3);
            });
            b.Services.RunLogsAtStartup();

            return b;
        }

        /// <summary>
        /// Access an ILogger using a required injected dependency of <typeparamref name="TDependency1"/>, <typeparamref name="TDependency2"/>, <typeparamref name="TDependency3"/>, and <typeparamref name="TDependency4"/>
        /// </summary>
        /// <remarks>
        /// If a required dependency is missing, an <see cref="InvalidOperationException"/> will be thrown
        /// </remarks>
        /// <typeparam name="TDependency1">Injected dependency</typeparam>
        /// <typeparam name="TDependency2">Injected dependency</typeparam>
        /// <typeparam name="TDependency3">Injected dependency</typeparam>
        /// <typeparam name="TDependency4">Injected dependency</typeparam>
        /// <param name="b"></param>
        /// <param name="action">Logger action</param>
        /// <returns></returns>
        public static DeferredLoggerBuilder ForRequired<TDependency1, TDependency2, TDependency3, TDependency4>(this DeferredLoggerBuilder b, Action<ILogger, TDependency1, TDependency2, TDependency3, TDependency4> action)
            where TDependency1 : class
            where TDependency2 : class
            where TDependency3 : class
            where TDependency4 : class
        {
            b.Services.AddOptions<StartupLoggerCache>().Configure<ILoggerFactory, TDependency1, TDependency2, TDependency3, TDependency4>((q, f, d1, d2, d3, d4) =>
            {
                var logger = q.LoggerFor(f, b.TypeName);
                action(logger, d1, d2, d3, d4);
            });
            b.Services.RunLogsAtStartup();

            return b;
        }
        #endregion

        #region Builders - For Optional Dependencies

        /// <summary>
        /// Access an ILogger using a nullable injected dependency of <typeparamref name="TDependency1"/>
        /// </summary>
        /// <remarks>
        /// If a required dependency is missing from the service provider, it will be null
        /// </remarks>
        /// <typeparam name="TDependency1">Injected dependency</typeparam>
        /// <param name="b"></param>
        /// <param name="action">Logger action</param>
        /// <returns></returns>
        public static DeferredLoggerBuilder For<TDependency1>(this DeferredLoggerBuilder b, Action<ILogger, TDependency1?> action)
            where TDependency1 : class
        {
            b.Services.AddOptions<StartupLoggerCache>().Configure<ILoggerFactory, IServiceProvider>((q, f, sp) =>
            {
                var d1 = sp.GetService<TDependency1>();
                var logger = q.LoggerFor(f, b.TypeName);
                action(logger, d1);
            });
            b.Services.RunLogsAtStartup();

            return b;
        }

        /// <summary>
        /// Access an ILogger using a nullable injected dependency of <typeparamref name="TDependency1"/> and <typeparamref name="TDependency2"/>
        /// </summary>
        /// <remarks>
        /// If a required dependency is missing from the service provider, it will be null
        /// </remarks>
        /// <typeparam name="TDependency1">Injected dependency</typeparam>
        /// <typeparam name="TDependency2">Injected dependency</typeparam>
        /// <param name="b"></param>
        /// <param name="action">Logger action</param>
        /// <returns></returns>
        public static DeferredLoggerBuilder For<TDependency1, TDependency2>(this DeferredLoggerBuilder b, Action<ILogger, TDependency1?, TDependency2?> action)
            where TDependency1 : class
            where TDependency2 : class
        {
            b.Services.AddOptions<StartupLoggerCache>().Configure<ILoggerFactory, IServiceProvider>((q, f, sp) =>
            {
                var d1 = sp.GetService<TDependency1>();
                var d2 = sp.GetService<TDependency2>();
                var logger = q.LoggerFor(f, b.TypeName);
                action(logger, d1, d2);
            });
            b.Services.RunLogsAtStartup();

            return b;
        }

        /// <summary>
        /// Access an ILogger using a nullable injected dependency of <typeparamref name="TDependency1"/>, <typeparamref name="TDependency2"/>, and <typeparamref name="TDependency3"/>
        /// </summary>
        /// <remarks>
        /// If a required dependency is missing from the service provider, it will be null
        /// </remarks>
        /// <typeparam name="TDependency1">Injected dependency</typeparam>
        /// <typeparam name="TDependency2">Injected dependency</typeparam>
        /// <typeparam name="TDependency3">Injected dependency</typeparam>
        /// <param name="b"></param>
        /// <param name="action">Logger action</param>
        /// <returns></returns>
        public static DeferredLoggerBuilder For<TDependency1, TDependency2, TDependency3>(this DeferredLoggerBuilder b, Action<ILogger, TDependency1?, TDependency2?, TDependency3?> action)
            where TDependency1 : class
            where TDependency2 : class
            where TDependency3 : class
        {
            b.Services.AddOptions<StartupLoggerCache>().Configure<ILoggerFactory, IServiceProvider>((q, f, sp) =>
            {
                var d1 = sp.GetService<TDependency1>();
                var d2 = sp.GetService<TDependency2>();
                var d3 = sp.GetService<TDependency3>();
                var logger = q.LoggerFor(f, b.TypeName);
                action(logger, d1, d2, d3);
            });
            b.Services.RunLogsAtStartup();

            return b;
        }

        /// <summary>
        /// Access an ILogger using a nullable injected dependency of <typeparamref name="TDependency1"/>, <typeparamref name="TDependency2"/>, <typeparamref name="TDependency3"/>, and <typeparamref name="TDependency4"/>
        /// </summary>
        /// <remarks>
        /// If a required dependency is missing from the service provider, it will be null
        /// </remarks>
        /// <typeparam name="TDependency1">Injected dependency</typeparam>
        /// <typeparam name="TDependency2">Injected dependency</typeparam>
        /// <typeparam name="TDependency3">Injected dependency</typeparam>
        /// <typeparam name="TDependency4">Injected dependency</typeparam>
        /// <param name="b"></param>
        /// <param name="action">Logger action</param>
        /// <returns></returns>
        public static DeferredLoggerBuilder For<TDependency1, TDependency2, TDependency3, TDependency4>(this DeferredLoggerBuilder b, Action<ILogger, TDependency1?, TDependency2?, TDependency3?, TDependency4?> action)
            where TDependency1 : class
            where TDependency2 : class
            where TDependency3 : class
            where TDependency4 : class
        {
            b.Services.AddOptions<StartupLoggerCache>().Configure<ILoggerFactory, IServiceProvider>((q, f, sp) =>
            {
                var d1 = sp.GetService<TDependency1>();
                var d2 = sp.GetService<TDependency2>();
                var d3 = sp.GetService<TDependency3>();
                var d4 = sp.GetService<TDependency4>();
                var logger = q.LoggerFor(f, b.TypeName);
                action(logger, d1, d2, d3, d4);
            });
            b.Services.RunLogsAtStartup();

            return b;
        }
        #endregion

        #region Extensions - ILogger Generic
        //------------------------------------------DEBUG------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogDebug(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogDebug(this IServiceCollection services, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Debug, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogDebug(0, "Processing request from {Address}", address)</example>
        public static void LogDebug(this IServiceCollection services, EventId eventId, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Debug, eventId, message, args));         
        }

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogDebug(exception, "Error while processing request from {Address}", address)</example>
        public static void LogDebug(this IServiceCollection services, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Debug, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogDebug("Processing request from {Address}", address)</example>
        public static void LogDebug(this IServiceCollection services, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Debug, message, args));
        }

        //------------------------------------------TRACE------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogTrace(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogTrace(this IServiceCollection services, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Trace, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogTrace(0, "Processing request from {Address}", address)</example>
        public static void LogTrace(this IServiceCollection services, EventId eventId, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Trace, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogTrace(exception, "Error while processing request from {Address}", address)</example>
        public static void LogTrace(this IServiceCollection services, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Trace, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogTrace("Processing request from {Address}", address)</example>
        public static void LogTrace(this IServiceCollection services, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Trace, message, args));
        }

        //------------------------------------------INFORMATION------------------------------------------//

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogInformation(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogInformation(this IServiceCollection services, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Information, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogInformation(0, "Processing request from {Address}", address)</example>
        public static void LogInformation(this IServiceCollection services, EventId eventId, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Information, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogInformation(exception, "Error while processing request from {Address}", address)</example>
        public static void LogInformation(this IServiceCollection services, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Information, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogInformation("Processing request from {Address}", address)</example>
        public static void LogInformation(this IServiceCollection services, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Information, message, args));
        }

        //------------------------------------------WARNING------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogWarning(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogWarning(this IServiceCollection services, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Warning, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogWarning(0, "Processing request from {Address}", address)</example>
        public static void LogWarning(this IServiceCollection services, EventId eventId, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Warning, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogWarning(exception, "Error while processing request from {Address}", address)</example>
        public static void LogWarning(this IServiceCollection services, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Warning, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogWarning("Processing request from {Address}", address)</example>
        public static void LogWarning(this IServiceCollection services, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Warning, message, args));
        }

        //------------------------------------------ERROR------------------------------------------//

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogError(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogError(this IServiceCollection services, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Error, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogError(0, "Processing request from {Address}", address)</example>
        public static void LogError(this IServiceCollection services, EventId eventId, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Error, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogError(exception, "Error while processing request from {Address}", address)</example>
        public static void LogError(this IServiceCollection services, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Error, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogError("Processing request from {Address}", address)</example>
        public static void LogError(this IServiceCollection services, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Error, message, args));
        }

        //------------------------------------------CRITICAL------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogCritical(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogCritical(this IServiceCollection services, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Critical, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogCritical(0, "Processing request from {Address}", address)</example>
        public static void LogCritical(this IServiceCollection services, EventId eventId, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Critical, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogCritical(exception, "Error while processing request from {Address}", address)</example>
        public static void LogCritical(this IServiceCollection services, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Critical, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogCritical("Processing request from {Address}", address)</example>
        public static void LogCritical(this IServiceCollection services, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(LogLevel.Critical, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log(this IServiceCollection services, LogLevel logLevel, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(logLevel, 0, null, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log(this IServiceCollection services, LogLevel logLevel, EventId eventId, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(logLevel, eventId, null, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log(this IServiceCollection services, LogLevel logLevel, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(logLevel, 0, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary>
        /// <param name="services">The startup service collection</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log(this IServiceCollection services, LogLevel logLevel, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog(logger => logger.Log(logLevel, eventId, exception, message, args));
        }
        #endregion

        #region Extensions - ILogger of T

        //------------------------------------------DEBUG------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogDebug(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogDebug<TLogger>(this IServiceCollection services, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Debug, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogDebug(0, "Processing request from {Address}", address)</example>
        public static void LogDebug<TLogger>(this IServiceCollection services, EventId eventId, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Debug, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogDebug(exception, "Error while processing request from {Address}", address)</example>
        public static void LogDebug<TLogger>(this IServiceCollection services, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Debug, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred debug log message.
        /// </summary>
        /// <typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogDebug("Processing request from {Address}", address)</example>
        public static void LogDebug<TLogger>(this IServiceCollection services, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Debug, message, args));
        }

        //------------------------------------------TRACE------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary>
        /// <typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogTrace(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogTrace<TLogger>(this IServiceCollection services, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Trace, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary>
        /// <typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogTrace(0, "Processing request from {Address}", address)</example>
        public static void LogTrace<TLogger>(this IServiceCollection services, EventId eventId, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Trace, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary>
        /// <typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogTrace(exception, "Error while processing request from {Address}", address)</example>
        public static void LogTrace<TLogger>(this IServiceCollection services, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Trace, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred trace log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogTrace("Processing request from {Address}", address)</example>
        public static void LogTrace<TLogger>(this IServiceCollection services, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Trace, message, args));
        }

        //------------------------------------------INFORMATION------------------------------------------//

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogInformation(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogInformation<TLogger>(this IServiceCollection services, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Information, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogInformation(0, "Processing request from {Address}", address)</example>
        public static void LogInformation<TLogger>(this IServiceCollection services, EventId eventId, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Information, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogInformation(exception, "Error while processing request from {Address}", address)</example>
        public static void LogInformation<TLogger>(this IServiceCollection services, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Information, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogInformation("Processing request from {Address}", address)</example>
        public static void LogInformation<TLogger>(this IServiceCollection services, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Information, message, args));
        }

        //------------------------------------------WARNING------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogWarning(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogWarning<TLogger>(this IServiceCollection services, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Warning, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogWarning(0, "Processing request from {Address}", address)</example>
        public static void LogWarning<TLogger>(this IServiceCollection services, EventId eventId, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Warning, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogWarning(exception, "Error while processing request from {Address}", address)</example>
        public static void LogWarning<TLogger>(this IServiceCollection services, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Warning, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred warning log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogWarning("Processing request from {Address}", address)</example>
        public static void LogWarning<TLogger>(this IServiceCollection services, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Warning, message, args));
        }

        //------------------------------------------ERROR------------------------------------------//

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogError(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogError<TLogger>(this IServiceCollection services, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Error, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogError(0, "Processing request from {Address}", address)</example>
        public static void LogError<TLogger>(this IServiceCollection services, EventId eventId, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Error, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogError(exception, "Error while processing request from {Address}", address)</example>
        public static void LogError<TLogger>(this IServiceCollection services, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Error, exception, message, args));
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogError("Processing request from {Address}", address)</example>
        public static void LogError<TLogger>(this IServiceCollection services, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Error, message, args));
        }

        //------------------------------------------CRITICAL------------------------------------------//

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogCritical(0, exception, "Error while processing request from {Address}", address)</example>
        public static void LogCritical<TLogger>(this IServiceCollection services, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Critical, eventId, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogCritical(0, "Processing request from {Address}", address)</example>
        public static void LogCritical<TLogger>(this IServiceCollection services, EventId eventId, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Critical, eventId, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogCritical(exception, "Error while processing request from {Address}", address)</example>
        public static void LogCritical<TLogger>(this IServiceCollection services, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Critical, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred critical log message.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>services.LogCritical("Processing request from {Address}", address)</example>
        public static void LogCritical<TLogger>(this IServiceCollection services, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(LogLevel.Critical, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log<TLogger>(this IServiceCollection services, LogLevel logLevel, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(logLevel, 0, null, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log<TLogger>(this IServiceCollection services, LogLevel logLevel, EventId eventId, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(logLevel, eventId, null, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log<TLogger>(this IServiceCollection services, LogLevel logLevel, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(logLevel, 0, exception, message, args));
        }

        /// <summary>
        /// Formats and writes a deferred log message at the specified log level.
        /// </summary><typeparam name="TLogger">The type to assign to the <see cref="ILogger"/></typeparam>
        /// <param name="services">The startup service collection</param>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log<TLogger>(this IServiceCollection services, LogLevel logLevel, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            services.DeferLog<TLogger>(logger => logger.Log(logLevel, eventId, exception, message, args));
        }
        #endregion
    }
}
