using Microsoft.Extensions.Logging;

namespace Tests
{
    internal class TestLoggerCache
    {
        public HashSet<string> Logs { get; set; } = new();

        public bool HasMatch(LogLevel level, string category, string log)
            => Logs.Contains(TestLogger.FormatLog(level, category, log));
    }
}