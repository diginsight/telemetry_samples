using Diginsight;
using Diginsight.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace SampleFunctionApp
{
    internal class Program
    {
        private static void Main()
        {
            AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);
            AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100));

            DiginsightActivitiesOptions activitiesOptions = new() { LogActivities = true };
            IDeferredLoggerFactory deferredLoggerFactory = new DeferredLoggerFactory(activitiesOptions: activitiesOptions);
            deferredLoggerFactory.ActivitySources.Add(Observability.ActivitySource);
            ILogger logger = deferredLoggerFactory.CreateLogger(typeof(Program));

            IHost host;
            using (Observability.ActivitySource.StartMethodActivity(logger))
            {
                host = new HostBuilder()
                    .ConfigureFunctionsWebApplication((context, workerAppBuilder) =>
                    {
                        var configuration = context.Configuration;
                        //workerAppBuilder.UseAspNetCoreIntegration();
                        workerAppBuilder.UseFunctionExecutionMiddleware();
                    })
                    .ConfigureFunctionsWorkerDefaults((context, workerAppBuilder) =>
                    {
                        //workerAppBuilder.Use;
                    })
                    .ConfigureAppConfigurationNH()
                    .ConfigureServices((hbc, services) => ConfigureServices(services, hbc.Configuration, hbc.HostingEnvironment, deferredLoggerFactory, logger))
                    .UseDiginsightServiceProvider()
                    .Build();
            }

            host.Run();
        }

        private static void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment,
            IDeferredLoggerFactory deferredLoggerFactory,
            ILogger logger
        )
        {
            using Activity? activity = Observability.ActivitySource.StartMethodActivity(logger);

            //services.AddAspNetCoreObservability(configuration, hostEnvironment);

            services.FlushOnCreateServiceProvider(deferredLoggerFactory);

            services.AddHttpClient(Options.DefaultName)
                .ConfigureHttpClient(
                    static (sp, hc) =>
                    {
                        //BomOptions bomOptions = sp.GetRequiredService<IOptions<BomOptions>>().Value;

                        //hc.Timeout = TimeSpan.FromMinutes(3);
                        //hc.BaseAddress = new Uri(bomOptions.BaseUrl);
                        //hc.DefaultRequestHeaders.Add(bomOptions.ApimHeader, bomOptions.Key);
                    }
                );

            //services.Configure<BomOptions>(configuration.GetSection("Bom"));
            //services.TryAddSingleton<BomFunctions>();
        }
    }
}
