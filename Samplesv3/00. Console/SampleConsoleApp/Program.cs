using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Diginsight;
using Diginsight.Diagnostics;
using Diginsight.Diagnostics.Log4Net;
using System.Text.Json;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System;
using OpenTelemetry.Metrics;

namespace SampleConsoleApp
{
    internal class Program
    {
        internal static readonly ActivitySource ActivitySource = new(typeof(Program).Namespace ?? typeof(Program).Name!);
        private static readonly JsonSerializerOptions MyJsonSerializerOptions = new(JsonSerializerOptions.Default) { ReadCommentHandling = JsonCommentHandling.Skip };

        public static IHost host;
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        private readonly ILoggerFactory loggerFactory;
        private readonly IFileProvider fileProvider;

        public Program(ILogger<Program> logger, IConfiguration configuration, ILoggerFactory loggerFactory, IFileProvider fileProvider)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.loggerFactory = loggerFactory;
            this.fileProvider = fileProvider;
        }

        private static async Task Main(string[] args)
        {
            //DiginsightActivitiesOptions activitiesOptions = new() { LogActivities = true, };
            //IDeferredLoggerFactory deferredLoggerFactory = new DeferredLoggerFactory(activitiesOptions: activitiesOptions);
            //var logger = deferredLoggerFactory.CreateLogger<Program>(); // this logger is deferred as configureLogging is not yet called

            //ActivitySource deferredActivitySource = deferredLoggerFactory.ActivitySource; // this activitySource is deferred as configureLogging is not yet called
            //using var deferredActivity = deferredActivitySource.StartMethodActivity(logger, new { args });

            DiginsightDefaults.ActivitySource = ActivitySource;

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .Build();
            //logger.LogDebug("configuration: {Configuration}", configuration);


            var appBuilder = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration(builder =>
                    {
                        //using var innerActivity = deferredActivitySource.StartMethodActivity(logger, new { builder });
                        builder.Sources.Clear();
                        builder.AddConfiguration(configuration);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        //using var innerActivity = deferredActivitySource.StartMethodActivity(logger, new { context, services });
                        //services.FlushOnCreateServiceProvider(deferredLoggerFactory);
                        
                        ConfigureServices(context.Configuration, services);
                    })
                    .ConfigureLogging((context, loggingBuilder) =>
                    {
                        //using var innerActivity = deferredActivitySource.StartMethodActivity(logger, new { context, loggingBuilder });

                        loggingBuilder.AddConfiguration(context.Configuration.GetSection("Logging"));

                        loggingBuilder.ClearProviders();

                        var services = loggingBuilder.Services;
                        services.AddLogging(
                                     loggingBuilder =>
                                     {
                                         loggingBuilder.ClearProviders();

                                         if (configuration.GetValue("AppSettings:ConsoleProviderEnabled", true))
                                         {
                                             loggingBuilder.AddDiginsightConsole();
                                         }

                                         if (configuration.GetValue("AppSettings:Log4NetProviderEnabled", true))
                                         {
                                             loggingBuilder.AddDiginsightLog4Net("log4net.config");
                                         }
                                     }
                                 );


                        services.ConfigureClassAware<DiginsightActivitiesOptions>(configuration.GetSection("Diginsight:Activities"));

                        services.AddSingleton<Program>();

                    });

            appBuilder.UseDiginsightServiceProvider(); // ensure opentelemetry ActivitySource listeners are registered (TracerProvider and MeterProvider), Flusies deferredLogger
            //logger.LogDebug("appBuilder.UseDiginsightServiceProvider(); completed");
            host = appBuilder.Build(); // logger.LogDebug("host = appBuilder.Build(); completed");

            var logger = host.Services.GetService<ILogger<Program>>();

            using var activity = Program.ActivitySource.StartMethodActivity(logger);

            logger.LogDebug("Sample app running section");


            activity.SetOutput(1); // traces span output
            return;
        }

        private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            //services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddHttpContextAccessor();
            //services.AddClassConfiguration();
            //services.EnsureDiginsight();


        }

    }
}
