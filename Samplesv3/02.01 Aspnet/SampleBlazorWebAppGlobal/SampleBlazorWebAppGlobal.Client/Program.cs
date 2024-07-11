using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Diginsight.Diagnostics;
using Diginsight;
using SampleBlazorWebAppGlobal.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var services = builder.Services;

//services.AddLogging(loggingBuilder =>
//{
//    //loggingBuilder.ClearProviders();
//    //loggingBuilder.AddConsole();
//    //loggingBuilder.AddDiginsightConsole();
//});

//services.Configure<DiginsightActivitiesOptions>(options =>
//{
//    options.LogActivities = true;
//    //options.ActivitySources.Add(Observability.ActivitySource.Name);
//});


builder.ConfigureContainer(new DiginsightServiceProviderFactory(new ServiceProviderOptions()));

var host = builder.Build();


await host.RunAsync();

