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
            //// See https://aka.ms/new-console-template for more information
            //Console.WriteLine("Hello, World, from sample console app!");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .Build();

            host = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration(builder =>
                    {
                        builder.Sources.Clear();
                        builder.AddConfiguration(configuration);
                        //builder.AddEnvironmentVariables();
                    }).ConfigureServices((context, services) =>
                    {
                        ConfigureServices(context.Configuration, services);
                    })
                    .ConfigureLogging((context, loggingBuilder) =>
                    {
                        DiginsightDefaults.ActivitySource = ActivitySource;

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

                        //var options = new Log4NetProviderOptions();
                        //options.Log4NetConfigFileName = "log4net.config";
                        //var log4NetProvider = new Log4NetProvider(options);
                        ////loggingBuilder.AddProvider(log4NetProvider);

                        services.ConfigureClassAware<DiginsightActivitiesOptions>(configuration.GetSection("Diginsight:Activities"));

                        var builder = services.AddDiginsightOpenTelemetry();

                        builder.WithTracing(
                            static tracerProviderBuilder =>
                            {
                                tracerProviderBuilder
                                    .AddDiginsight()
                                    .AddSource(ActivitySource.Name)
                                    .SetSampler(new AlwaysOnSampler());
                            }
                        );

                        services.AddSingleton<Program>();

                    }).Build();

            var logger = host.Services.GetService<ILogger<Program>>();

            using var activity = Program.ActivitySource.StartMethodActivity(logger);

            logger.LogDebug("Sample app running section");


            return;
        }


        private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            //services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddHttpContextAccessor();
            //services.AddClassConfiguration();


        }

    }
}
