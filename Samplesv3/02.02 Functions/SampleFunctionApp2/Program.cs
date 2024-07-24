using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(SampleFunctionApp2.Startup))]

namespace SampleFunctionApp2;

public class Startup : FunctionsStartup
{
    //public static IDeferredLoggerFactory DeferredLoggerFactory;

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        FunctionsHostBuilderContext context = builder.GetContext();

        builder.ConfigurationBuilder
            .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
            .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
            //.AddUserSecrets<Startup>();

    }
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;


        //builder.Services.AddHttpClient();

        //builder.Services.AddSingleton<IMyService>((s) => {
        //    return new MyService();
        //});

        //builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();
    }
}