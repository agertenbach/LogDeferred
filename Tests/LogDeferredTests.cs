using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Microsoft.Extensions.DependencyInjection.DeferredLoggerExtensions;

namespace Tests
{
    public class LogDeferredTests
    {
        [Fact]
        public void IServiceCollection_Logging_Works_StringOnly()
        {
            // Arrange
            var fixture = new LogDeferredFixture();
            string logString = "Log a debug message";

            // Act
            fixture.Builder.Services.LogDebug(logString);
            fixture.Start();

            // Assert
            var match = fixture.Cache.HasMatch(LogLevel.Debug, StartupLoggerCache.DefaultStartupLoggerName, logString);
            Assert.True(match);
            fixture.Dispose();
        }

        [Fact]
        public void IServiceCollection_Logging_Works_CustomDefault()
        {
            // Arrange
            var fixture = new LogDeferredFixture();
            string logString = "Log a debug message";
            string loggerName = "logger name";

            // Act
            fixture.Builder.Services.SetStartupLoggerName(loggerName);
            fixture.Builder.Services.LogDebug(logString);
            fixture.Start();

            // Assert
            var match = fixture.Cache.HasMatch(LogLevel.Debug, loggerName, logString);
            Assert.True(match);
            fixture.Dispose();
        }

        [Fact]
        public void IServiceCollection_Logging_Works_CustomLogger()
        {
            // Arrange
            var fixture = new LogDeferredFixture();
            string logString = "Log a debug message";
            string loggerName = typeof(LogDeferredTests).FullName ?? typeof(LogDeferredTests).Name;

            // Act
            fixture.Builder.Services.LogDebug<LogDeferredTests>(logString);
            fixture.Start();

            // Assert
            var match = fixture.Cache.HasMatch(LogLevel.Debug, loggerName, logString);
            Assert.True(match);
            fixture.Dispose();
        }

        [Fact]
        public void IServiceCollection_Logging_Works_StringWithArgs()
        {
            // Arrange
            var fixture = new LogDeferredFixture();
            string logStringFormat = "Log a debug message with {0}";
            string logStringArg = "argument";

            // Act
            fixture.Builder.Services.LogDebug(logStringFormat, logStringArg);
            fixture.Start();

            // Assert
            var match = fixture.Cache.HasMatch(LogLevel.Debug, StartupLoggerCache.DefaultStartupLoggerName, string.Format(logStringFormat, logStringArg)); Assert.True(match);
            Assert.True(match);
            fixture.Dispose();         
        }

        [Fact]
        public void IServiceCollection_Logging_Works_StringWithArgsAndException()
        {
            // Arrange
            var fixture = new LogDeferredFixture();
            string logStringFormat = "Log a critical message with {0}";
            string logStringArg = "exception message";

            // Act
            try
            {
                throw new Exception(logStringArg);
            }
            catch (Exception ex)
            {
                fixture.Builder.Services.LogCritical(ex, logStringFormat, ex.Message);
            }
            fixture.Start();

            // Assert
            var match = fixture.Cache.HasMatch(LogLevel.Critical, StartupLoggerCache.DefaultStartupLoggerName, string.Format(logStringFormat, logStringArg)); Assert.True(match);
            Assert.True(match);
            fixture.Dispose();
        }

        [Fact]
        public void IServiceCollection_Logging_Works_OptionalDeps()
        {
            // Arrange
            var fixture = new LogDeferredFixture();
            string logStringFormat = "Log a debug message with {0}, {1}";
            string propValue = "expected prop value";
            fixture.Builder.Services.Configure<TestOptionsOne>(o => o.Prop = propValue);
            fixture.Builder.Services.AddSingleton(new TestOptionsTwo() { Prop = propValue });

            // Act
            fixture.Builder.Services.Log().For<IOptions<TestOptionsOne>, TestOptionsTwo>((logger, opts, singleton) =>
            {
                logger.LogDebug(logStringFormat, opts?.Value.Prop, singleton?.Prop);
            });
            fixture.Start();

            // Assert
            var match = fixture.Cache.HasMatch(LogLevel.Debug, StartupLoggerCache.DefaultStartupLoggerName, string.Format(logStringFormat, propValue, propValue)); Assert.True(match);
            Assert.True(match);
            fixture.Dispose();
        }

        [Fact]
        public void IServiceCollection_Logging_Works_OptionalDepsDeferred()
        {
            // Arrange
            var fixture = new LogDeferredFixture();
            string logStringFormat = "Log a debug message with {0}, {1}";
            string propValue = "expected prop value";
            string updatedPropValue = "updated prop value";
            fixture.Builder.Services.Configure<TestOptionsOne>(o => o.Prop = propValue);
            fixture.Builder.Services.AddSingleton(new TestOptionsTwo() { Prop = propValue });

            // Act
            fixture.Builder.Services.Log().For<IOptions<TestOptionsOne>, TestOptionsTwo>((logger, opts, singleton) =>
            {
                logger.LogDebug(logStringFormat, opts?.Value.Prop, singleton?.Prop);
            });
            fixture.Builder.Services.Configure<TestOptionsOne>(o => o.Prop = updatedPropValue); // This should happen before the logger runs
            fixture.Start();

            // Assert
            var match = fixture.Cache.HasMatch(LogLevel.Debug, StartupLoggerCache.DefaultStartupLoggerName, string.Format(logStringFormat, updatedPropValue, propValue)); Assert.True(match);
            Assert.True(match);
            fixture.Dispose();
        }

        [Fact]
        public void IServiceCollection_Logging_Works_RequiredDepsT()
        {
            // Arrange
            var fixture = new LogDeferredFixture();
            string logStringFormat = "Log a debug message with {0}, {1}";
            string propValue = "expected prop value";
            fixture.Builder.Services.Configure<TestOptionsOne>(o => o.Prop = propValue);
            fixture.Builder.Services.AddSingleton(new TestOptionsTwo() { Prop = propValue });
            string loggerName = typeof(LogDeferredTests).FullName ?? typeof(LogDeferredTests).Name;

            // Act
            fixture.Builder.Services.Log<LogDeferredTests>().ForRequired<IOptions<TestOptionsOne>, TestOptionsTwo>((logger, opts, singleton) =>
            {
                logger.LogDebug(logStringFormat, opts.Value.Prop, singleton.Prop);
            });
            fixture.Start();

            // Assert
            var match = fixture.Cache.HasMatch(LogLevel.Debug, loggerName, string.Format(logStringFormat, propValue, propValue)); Assert.True(match);
            Assert.True(match);
            fixture.Dispose();
        }

        [Fact]
        public void IServiceCollection_Logging_RequiredDepsMissing_InvalidOperationException()
        {
            // Arrange
            var fixture = new LogDeferredFixture();
            string logStringFormat = "Log a debug message with {0}";
            string loggerName = typeof(LogDeferredTests).FullName ?? typeof(LogDeferredTests).Name;

            // Act
            fixture.Builder.Services.Log<LogDeferredTests>().ForRequired<TestOptionsTwo>((logger,singleton) =>
            {
                logger.LogDebug(logStringFormat, singleton.Prop);
            });
            
            // Assert
            Assert.Throws<InvalidOperationException>(() => fixture.Start());
            fixture.Dispose();
        }

        [Fact]
        public void IServiceCollection_Logging_Works_RequiredDepsTDeferred()
        {
            // Arrange
            var fixture = new LogDeferredFixture();
            string logStringFormat = "Log a debug message with {0}, {1}";
            string propValue = "expected prop value";
            string updatedPropValue = "updated prop value";
            fixture.Builder.Services.Configure<TestOptionsOne>(o => o.Prop = propValue);
            fixture.Builder.Services.AddSingleton(new TestOptionsTwo() { Prop = propValue });
            string loggerName = typeof(LogDeferredTests).FullName ?? typeof(LogDeferredTests).Name;

            // Act
            fixture.Builder.Services.Log<LogDeferredTests>().ForRequired<IOptions<TestOptionsOne>, TestOptionsTwo>((logger, opts, singleton) =>
            {
                logger.LogDebug(logStringFormat, opts.Value.Prop, singleton.Prop);
            });
            fixture.Builder.Services.Configure<TestOptionsOne>(o => o.Prop = updatedPropValue); // This should happen before the logger runs
            fixture.Start();

            // Assert
            var match = fixture.Cache.HasMatch(LogLevel.Debug, loggerName, string.Format(logStringFormat, updatedPropValue, propValue)); Assert.True(match);
            Assert.True(match);
            fixture.Dispose();
        }
    }
}