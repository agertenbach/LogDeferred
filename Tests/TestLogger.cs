using Microsoft.Extensions.Logging;

namespace Tests
{
    internal class TestLogger : ILogger
    {
        private readonly TestLoggerCache _logs;
        private readonly string _category;

        public TestLogger(TestLoggerCache logs, string category)
        {
            _logs = logs ?? throw new ArgumentNullException(nameof(logs));
            _category = category ?? throw new ArgumentNullException(nameof(category));
        }

        public static string FormatLog(LogLevel level, string category, string logString)
            => $"[{level}] {category} : {logString}";

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var logstring = formatter(state, exception);
            if (logstring is not null) _logs.Logs.Add(FormatLog(logLevel, _category, logstring));
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => throw new NotSupportedException();
    }
}