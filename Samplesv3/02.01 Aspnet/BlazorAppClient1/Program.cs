using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorAppClient1;
using Diginsight.Diagnostics;
using Diginsight;
using Diginsight.Strings;
using System.Diagnostics;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var services = builder.Services;

services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    // loggingBuilder.AddConsole();
    // loggingBuilder.AddDiginsightConsole();

    var emitter = new BlazorActivityLogEmitter();
    var activityListener = emitter.ToActivityListener(static _ => true);
    ActivitySource.AddActivityListener(activityListener);
});

services.Configure<DiginsightActivitiesOptions>(options =>
{
    options.LogActivities = true;
    // options.ActivitySources.Add(Observability.ActivitySource.Name);
});

services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
});

builder.ConfigureContainer(new DiginsightServiceProviderFactory(new ServiceProviderOptions()));
var host = builder.Build();
await host.RunAsync();











