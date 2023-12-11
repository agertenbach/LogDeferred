using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Tests
{
    internal class TestLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new();
        private readonly TestLoggerCache _cache;

        public TestLoggerProvider(TestLoggerCache cache) => _cache = cache;

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, new TestLogger(_cache, categoryName));
        }

        public void Dispose()
        {
        }
    }
}