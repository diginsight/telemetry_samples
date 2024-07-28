using System.IO;
using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Microsoft.Azure.Functions.Samples.DependencyInjectionBasic.SampleStartup))]

namespace Microsoft.Azure.Functions.Samples.DependencyInjectionBasic
{
    public class SampleStartup : FunctionsStartup
    {
        //public static ILoggerFactory LoggerFactory;
        //private readonly ILogger logger;

        //private static readonly JsonSerializerOptions MyJsonSerializerOptions = new(JsonSerializerOptions.Default) { ReadCommentHandling = JsonCommentHandling.Skip };
        //public static IHost host;
        //private readonly IConfiguration configuration;
        //private readonly IFileProvider fileProvider;

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            //var activitiesOptions = new DiginsightActivitiesOptions() { LogActivities = true };
            //var deferredLoggerFactory = new DeferredLoggerFactory(activitiesOptions: activitiesOptions);
            //deferredLoggerFactory.ActivitySources.Add(Observability.ActivitySource);
            //LoggerFactory = deferredLoggerFactory;
            //var logger = LoggerFactory.CreateLogger<Program>();

            //using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { args });

            FunctionsHostBuilderContext context = builder.GetContext();

            // Note that these files are not automatically copied on build or publish. 
            // See the csproj file to for the correct setup.
            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddUserSecrets<SampleStartup>();
                

        }

        public override void Configure(IFunctionsHostBuilder builder)
        {


            builder.Services.AddScoped<IGreeter, SampleGreeter>();
            FunctionsHostBuilderContext context = builder.GetContext();
            
            var configuration = context.Configuration;


            builder.Services.Configure<SampleOptions>(context.Configuration);
        }

    }
}
