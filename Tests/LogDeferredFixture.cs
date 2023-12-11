using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tests
{
    internal class LogDeferredFixture : IDisposable
    {
        public WebApplicationBuilder Builder { get; }

        private WebApplication? _app;

        private bool _hasStarted;

        public TestLoggerCache Cache { get; } = new TestLoggerCache();

        public LogDeferredFixture()
        {
            Builder = WebApplication.CreateSlimBuilder();
            Builder.Logging.ClearProviders();
            var provider = new TestLoggerProvider(Cache);
            Builder.Logging.AddProvider(provider);
            Builder.Logging.SetMinimumLevel(LogLevel.Trace);
        }

        public void Start()
        {
            if (!_hasStarted)
            {
                _hasStarted = true;
                _app = Builder.Build();
                _app.Start();
            }    
        }


        public void Dispose()
        {
            _app?.StopAsync();
            _app?.DisposeAsync();
        }
    }
}