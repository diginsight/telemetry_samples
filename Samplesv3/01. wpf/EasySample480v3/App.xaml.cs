#region using
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Windows;
using Diginsight.Diagnostics;
using Microsoft.Extensions.Logging;
using Diginsight;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Diginsight.Diagnostics.Log4Net;
using System.Runtime.CompilerServices;
using log4net.Appender;
using System.IO;

namespace EasySample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IDeferredLoggerFactory DeferredLoggerFactory;
        public static IHost Host;

        static App()
        {
            var activitiesOptions = new DiginsightActivitiesOptions() { LogActivities = true };
            DeferredLoggerFactory = new DeferredLoggerFactory(activitiesOptions: activitiesOptions);
            DeferredLoggerFactory.ActivitySources.Add(Observability.ActivitySource);
            var logger = DeferredLoggerFactory.CreateLogger<App>();

            using var activity = Observability.ActivitySource.StartMethodActivity(logger);


        }

        public App()
        {
            var logger = DeferredLoggerFactory.CreateLogger<App>();
            using var activity = Observability.ActivitySource.StartMethodActivity(logger);


        }
        protected override async void OnStartup(StartupEventArgs e)
        {
            var logger = DeferredLoggerFactory.CreateLogger<App>();
            using var activity = Observability.ActivitySource.StartMethodActivity(logger);



            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ;
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<App>()
                .Build();
            logger.LogDebug($"var configuration = new ConfigurationBuilder()....Build() comleted");
            logger.LogDebug("environment:{environment},configuration:{Configuration}", environment, configuration);

            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(  )
                .ConfigureServices((context, services) =>
                {
                    services.FlushOnCreateServiceProvider(DeferredLoggerFactory);
                    services.AddSingleton<MainWindow>();
                })
                .ConfigureLogging((context, loggingBuilder) =>
                {

                    loggingBuilder.AddConfiguration(context.Configuration.GetSection("Logging"));
                    loggingBuilder.ClearProviders();
                    var services = loggingBuilder.Services;
                    services.AddLogging(
                                 loggingBuilder1 =>
                                 {
                                     loggingBuilder1.ClearProviders();

                                     //loggingBuilder.AddDiginsightLog4Net("log4net.config");
                                     loggingBuilder.AddDiginsightLog4Net(static sp =>
                                     {
                                         IHostEnvironment env = sp.GetRequiredService<IHostEnvironment>();
                                         //string fileBaseDir = env.IsDevelopment()
                                         //        ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify)
                                         //        : $"{Path.DirectorySeparatorChar}home";
                                         string fileBaseDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify);

                                         return new IAppender[]
                                                {
                                                            new RollingFileAppender()
                                                            {
                                                                File = Path.Combine(fileBaseDir, "LogFiles", "Diginsight", typeof(App).Namespace!),
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
                                     static _ => log4net.Core.Level.All);
                                 }
                             );
                    services.ConfigureClassAware<DiginsightActivitiesOptions>(configuration.GetSection("Diginsight:Activities"));
                    services.AddSingleton<App>();

                })
                .UseDiginsightServiceProvider()
                .Build();

            await Host.StartAsync();

            logger.LogDebug("host = appBuilder.Build(); completed");
            var mainWindow = Host.Services.GetRequiredService<MainWindow>(); logger.LogDebug($"Host.Services.GetRequiredService<MainWindow>(); returns {mainWindow.ToLogString()}");

            mainWindow.Show(); logger.LogDebug($"mainWindow.Show();");
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            var logger = DeferredLoggerFactory.CreateLogger<App>();
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { e });

            using (Host)
            {
                await Host.StopAsync(TimeSpan.FromSeconds(5));
            }

            base.OnExit(e);
        }

    }
}
#endregion