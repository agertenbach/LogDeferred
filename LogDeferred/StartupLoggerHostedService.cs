using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Microsoft.Extensions.DependencyInjection.DeferredLoggerExtensions;

namespace DeferredLog.Hosting
{
    /// <summary>
    /// Hosted service to invoke logs
    /// </summary>
    internal sealed class StartupLoggerHostedService : IHostedService
    {
        private readonly IOptions<StartupLoggerCache> _loggerCache;
        private readonly ILoggerFactory _lf;

        public StartupLoggerHostedService(IOptions<StartupLoggerCache> loggerCache, ILoggerFactory lf)
        {
            _loggerCache = loggerCache;
            _lf = lf;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            string name = _loggerCache.Value.StartupLoggerName ?? StartupLoggerCache.DefaultStartupLoggerName;
            var logger = _loggerCache.Value.Loggers.GetOrAdd(name, _lf.CreateLogger(name));
            logger.LogTrace("Startup logging complete");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}