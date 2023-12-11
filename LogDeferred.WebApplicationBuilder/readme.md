# LogDeferred

## What's the deal with startup logging?
Modern ASP NET Core applications are generally built around a `Program.cs` file that includes a host building process with services registration followed by configuration of the HTTP request pipeline. 

    var builder = WebApplication.CreateBuilder(args);
    // Add services to the container...
    builder.Services.AddControllers();
    
    var app = builder.Build();
    // Configure the HTTP request pipeline...
    app.Run();

At any point after the `WebApplication` is built, the service provider is available and logging can be accessed using code similar to the following:

    var app = builder.Build();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Here is a log entry after app build");

However, between the creation of the builder and the call to `Build()`, services registration is still occurring and it's not possible to use the service provider to obtain an `ILogger`.  Despite DI-oriented abstractions, loggers are typically available as a global static reference and the traditional solution for access to logging at this phase is to bypass the `Microsoft.Logging.Extensions.ILogger` abstraction and leverage the static logger you are implementing directly, i.e. the `Serilog.ILogger`. This works, but it comes with its own shortcomings: you're now calling a concrete implementation and the global logger may still be in a bootstrapping phase with incomplete configuration, for example if you use an `appsettings.json` file to set verbosity and sinks.

If you're attempting to log the state of the web application builder itself prior to the start of the host, this remains the primary option, but sometimes it would be beneficial to have access to the familiar `ILogger` during this phase to help validate registration processes occurred as expected. In larger applications, the service registration process may be quite involved with logical branches based on configuration across several sources such as `appsettings.json` files, `secrets.json`, Azure Key Vault and more, often including binding or progressive actions applied to options classes. These can be more difficult to debug when your application is deployed onto a container or remote machine.

## What does this package do?
We can't make the fully configured `ILogger` available during that first phase, but we *can* compose and capture actions that we want to run against the `ILogger` that can be deferred until the host starts. This package uses the progressive nature of the `IOptions` registration process combined with some extension methods to expose the familiar `ILogger` interfaces against the `IServiceCollection` directly. Logs are published to the fully configured `ILogger` as soon as the host starts through the use of an `IHostedService` to invoke the pending `IOptions` actions.

In addition to the ordinary `ILogger`, we can add some additional extensions that take advantage of the ability to inject composed dependencies without needing to instantiate explicit classes or validators in order to access state needed for logging at startup.

### Example usage
Consider the following (very contrived) example:

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllersWithViews();
    
    try
    {
        var myConfigurationSection = builder.Configuration.GetRequiredSection("MyConfiguration");
        var myConfiguration = myConfigurationSection.Get<MyConfiguration>();
        if (myConfiguration is null)
        {
            builder.Services.LogError("{configuration} was null", nameof(myConfiguration));
        }
        else
        {
            builder.Services.AddOptions<MyConfiguration>().Bind(myConfigurationSection);
            if (myConfiguration.UseFeatureOne)
            {
                builder.Services.LogInformation("Feature One is enabled");
                builder.Services.AddTransient<FeatureOne>();
            }
    
            switch (myConfiguration.DatabaseProvider)
            {
                case DatabaseProvider.Local:
                    builder.Services.LogInformation<DatabaseProvider>("Database provider is {provider}", DatabaseProvider.Local.ToString());
                    builder.Services.AddScoped<IDatabaseProvider, LocalDatabaseProvider>();
                    builder.Services.AddOptions<DatabaseOptions>().BindConfiguration(nameof(DatabaseOptions)).PostConfigure(c => c.Mode = 1);
                    break;
                case DatabaseProvider.Cloud:
                    builder.Services.LogInformation<DatabaseProvider>("Database provider is {provider}", DatabaseProvider.Cloud.ToString());
                    builder.Services.AddScoped<IDatabaseProvider, CloudDatabaseProvider>();
                    builder.Services.AddOptions<DatabaseOptions>().BindConfiguration(nameof(DatabaseOptions)).PostConfigure(c => c.Mode = 2);
                    break;
                default:
                    builder.Services.LogInformation<DatabaseProvider>("No database provider was configured!");
                    break;
            }
    
            builder.Services.Log<DatabaseProvider>()
                .For<IOptions<DatabaseOptions>, FeatureOne>((logger, options, feature) =>
                    {
                        if ((options?.Value.IsValid() ?? false) && (feature?.IsLicensed ?? false))
                        {
                            logger.LogDebug("Feature one is licensed for database usage against {connString}", options.Value.ConnectionString);
                        }
                        else logger.LogWarning("Feature one doesn't have a valid connection string!");
                    })
                .ForRequired<IOptionsMonitor<MyConfiguration>>((logger, optMonitor) =>
                    {
                        logger.LogInformation("Here is other useful DatabaseProvider information");
                    });
        }
        
    }
    catch(Exception ex)
    {
        builder.Services.LogCritical(ex, "The registration for {configuration} threw an exception", nameof(MyConfiguration));
    }
    
    var app = builder.Build();

With the deferred logging available from the start, I can get a better picture of how the composition is being handled and what the final state of my configuration objects are:

    info: Microsoft.Hosting.Startup[0]
          Feature One is enabled
    info: MyApplication.Services.DatabaseProvider[0]
          Database provider is Local
    info: MyApplication.Services.FeatureOne[0]
          Feature one doesn't have a valid connection string!
    info: MyApplication.Services.DatabaseProvider[0]
          Here is other useful DatabaseProvider information
    info: Microsoft.Hosting.Lifetime[14]
          Now listening on: https://localhost:5001
    info: Microsoft.Hosting.Lifetime[14]
          Now listening on: http://localhost:5000
    info: Microsoft.Hosting.Lifetime[0]
          Application started. Press Ctrl+C to shut down.
    info: Microsoft.Hosting.Lifetime[0]
          Hosting environment: Development

### Basic usage
All the familiar `ILogger` behavior is available directly against the IServiceCollection, with the option to specify a type parameter that will be used to set the logger category for the entry:

    builder.Services.LogDebug("I will show up under the default Microsoft.Hosting.Startup logger");
    builder.Services.LogInformation<MyTypeHere>("I will show up under the {type} logger", typeof(MyTypeHere).FullName);

One or more actions can be called against the resolved `ILogger` itself as an action, instead:


    builder.Services.Log(logger =>
    {
        logger.LogInformation("Here's one log");
        logger.LogDebug("Here's another!");
    });
    
    builder.Services.Log<MyTypeHere>(logger =>
    {
        logger.LogInformation("Types are supported here too");
    });

If your log actions require injected dependencies, you may chain them using the following syntax for optional/nullable and required dependencies respectively:

    builder.Services.Log<MyFeatures>()
        .For<FeatureOne>((logger, feature) =>
            {
                logger.LogInformation("Feature one is null? {isNull}", feature is null);
            })
        .ForRequired<FeatureTwo, IOptions<MyFeatureConfig>>((logger, f2, config) =>
            {
                logger.LogInformation("If either of these were null, you'd get an InvalidOperationException");
            });

### Important notes about usage of injected dependencies
Because the logging is being deferred until the host starts, actions are not run immediately. For the extensions that inject additional dependencies, this means that the log will consider the final state of the object, not the state as it existed at the time the log action was registered.

For example:

    builder.Services.Configure<DatabaseOptions>(o => o.Mode = 1);
    builder.Services.Log<DatabaseOptions>()
        .ForRequired<IOptions<DatabaseOptions>>((logger, options) =>
           logger.LogDebug("Mode is {mode}", options.Value.Mode);
        );
    builder.Services.Configure<DatabaseOptions>(o => o.Mode = 2);

will result in the log `Mode is 2` because both calls to `Configure` occur before the host starts and the value is computed.

### Customizing the default startup logger name
Since the extensions are running against the `IServiceCollection` directly, we must specify a default logger category for any log events if a type parameter is not supplied. To keep the naming approximately consistent with other logging occurring as the host starts, we opted for "Microsoft.Hosting.Startup". 

If you would like to use an alternative logger name, you may call the following prior to any generic logging action:

    builder.Services.SetStartupLoggerName("MyStartupLoggerName");

### LogDeferred.WebApplicationBuilder
In order to minimize the dependencies and maximize compatibility of the library, the core package targets the `IServiceCollection`. If you're using ASP NET Core 6 or higher and explicitly interacting with a `WebApplicationBuilder`, you may optionally use the `LogDeferred.WebApplicationBuilder` package instead, which adds the same extensions to the builder itself.

For example, `builder.Services.LogInformation()` can be shortened to `builder.LogInformation()`

## License
MIT License
Copyright (c) 2023 Adam Gertenbach

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.