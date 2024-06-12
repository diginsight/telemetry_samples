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
using log4net.Appender;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SampleConsoleApp
{
    internal class Program
    {
        internal static ILoggerFactory LoggerFactory { get; private set; }
        internal static readonly ActivitySource ActivitySource = new(typeof(Program).Namespace ?? typeof(Program).Name!);
        private static readonly JsonSerializerOptions MyJsonSerializerOptions = new(JsonSerializerOptions.Default) { ReadCommentHandling = JsonCommentHandling.Skip };

        public static IHost host;
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        private readonly ILoggerFactory loggerFactory;
        private readonly IFileProvider fileProvider;

        public Program(ILogger<Program> logger, IConfiguration configuration, ILoggerFactory loggerFactory, IFileProvider fileProvider)
        {
            DiginsightActivitiesOptions activitiesOptions = new() { LogActivities = true };
            var deferredLoggerFactory = new DeferredLoggerFactory(activitiesOptions: activitiesOptions);
            deferredLoggerFactory.ActivitySources.Add(ActivitySource);
            LoggerFactory = deferredLoggerFactory;
            logger = LoggerFactory.CreateLogger<Program>();

            using var activity = ActivitySource.StartMethodActivity(logger);
            try
            {
                this.logger = logger;
                this.configuration = configuration;
                this.loggerFactory = loggerFactory;
                this.fileProvider = fileProvider;
            }
            catch (Exception /*ex*/) { /*sec.Exception(ex);*/ }
        }

        private static async Task Main(string[] args)
        {
            DiginsightActivitiesOptions activitiesOptions = new() { LogActivities = true };
            var deferredLoggerFactory = new DeferredLoggerFactory(activitiesOptions: activitiesOptions);
            deferredLoggerFactory.ActivitySources.Add(ActivitySource);
            LoggerFactory = deferredLoggerFactory;
            var logger = LoggerFactory.CreateLogger<Program>();

            using var activity = ActivitySource.StartMethodActivity(logger, new { args });

            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .Build();
            logger.LogDebug($"var configuration = new ConfigurationBuilder()....Build() comleted");
            logger.LogDebug("environment:{environment},configuration:{Configuration}", environment, configuration);

            var appBuilder = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration(builder =>
                    {
                        using var innerActivity = ActivitySource.StartRichActivity(logger, "ConfigureAppConfiguration.Callback", new { builder });

                        builder.Sources.Clear();
                        builder.AddConfiguration(configuration);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        using var innerActivity = ActivitySource.StartRichActivity(logger, "ConfigureServices.Callback", new { context, services });
                        services.TryAddSingleton(deferredLoggerFactory);
                        services.FlushOnCreateServiceProvider(deferredLoggerFactory);

                        ConfigureServices(context.Configuration, services);
                    })
                    .ConfigureLogging((context, loggingBuilder) =>
                    {
                        using var innerActivity = ActivitySource.StartRichActivity(logger, "ConfigureLogging.Callback", new { context, loggingBuilder });

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
                                             //loggingBuilder.AddDiginsightLog4Net("log4net.config");
                                             loggingBuilder.AddDiginsightLog4Net(static sp =>
                                                                                 {
                                                                                     IHostEnvironment env = sp.GetRequiredService<IHostEnvironment>();
                                                                                     string fileBaseDir = env.IsDevelopment()
                                                                                         ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify)
                                                                                         : $"{Path.DirectorySeparatorChar}home";

                                                                                     return new IAppender[]
                                                                                     {
                                                                                            new RollingFileAppender()
                                                                                            {
                                                                                                File = Path.Combine(fileBaseDir, "LogFiles", "Diginsight", typeof(Program).Namespace!),
                                                                                                AppendToFile = true,
                                                                                                StaticLogFileName = false,
                                                                                                RollingStyle = RollingFileAppender.RollingMode.Composite,
                                                                                                DatePattern = @".yyyyMMdd.\l\o\g",
                                                                                                MaxSizeRollBackups = 1000,
                                                                                                MaximumFileSize = "100MB",
                                                                                                LockingModel = new FileAppender.MinimalLock(),
                                                                                                Layout = new DiginsightLayout()
                                                                                                {
                                                                                                    Pattern = "{Timestamp} {Category} {LogLevel} {TraceId} {Delta} {Duration} {Depth} {Indentation|-1} {Message}",
                                                                                                },
                                                                                            },
                                                                                     };
                                                                                 },
                                                                                 static _ => log4net.Core.Level.All
                                                                             );
                                         }
                                     }
                                 );

                        services.ConfigureClassAware<DiginsightActivitiesOptions>(configuration.GetSection("Diginsight:Activities"));
                        services.AddSingleton<Program>();
                    });

            appBuilder.UseDiginsightServiceProvider(); // ensure opentelemetry ActivitySource listeners are registered (TracerProvider and MeterProvider), Flusies deferredLogger
            logger.LogDebug("appBuilder.UseDiginsightServiceProvider(); completed");

            host = appBuilder.Build();
            logger.LogDebug("host = appBuilder.Build(); completed");

            //logger = host.Services.GetService<ILogger<Program>>();
            //using var activity = Program.ActivitySource.StartMethodActivity(logger);
            logger.LogDebug("Sample app running section");


            activity?.SetOutput(1); // traces span output

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
