using Diginsight.Diagnostics;
using Microsoft.Extensions.Logging;
using SampleBlazorApp;
using SampleBlazorApp.Client.Pages;
using SampleBlazorApp.Components;
using System.Diagnostics;


var activitiesOptions = new DiginsightActivitiesOptions() { LogActivities = true };
var deferredLoggerFactory = new DeferredLoggerFactory(activitiesOptions: activitiesOptions);
//deferredLoggerFactory.ActivitySources.Add(Observability.ActivitySource);
deferredLoggerFactory.ActivitySourceFilter = (activitySource) => activitySource.Name.StartsWith($"Sample");
var logger = deferredLoggerFactory.CreateLogger(typeof(Program));

using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { args });

var builder = WebApplication.CreateBuilder(args); logger.LogDebug($"var builder = WebApplication.CreateBuilder(args);");

// Add services to the container.
builder.Services.AddRazorComponents()
       .AddInteractiveServerComponents()
       .AddInteractiveWebAssemblyComponents();
logger.LogDebug($"builder.Services.AddRazorComponents();");
logger.LogDebug($"builder.Services.AddInteractiveServerComponents();");
logger.LogDebug($"builder.Services.AddInteractiveWebAssemblyComponents();");

var app = builder.Build(); logger.LogDebug($"var app = builder.Build();");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging(); logger.LogDebug($"app.UseWebAssemblyDebugging();");
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true); logger.LogDebug($"app.UseExceptionHandler(\"/Error\", createScopeForErrors: true);");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts(); logger.LogDebug($"app.UseHsts();");
}

app.UseHttpsRedirection(); logger.LogDebug($"app.UseHttpsRedirection();");

app.UseStaticFiles(); logger.LogDebug($"app.UseStaticFiles();");
app.UseAntiforgery(); logger.LogDebug($"app.UseAntiforgery();");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(SampleBlazorApp.Client._Imports).Assembly);
logger.LogDebug($"app.MapRazorComponents<App>();");
logger.LogDebug($"app.AddInteractiveServerRenderMode()");
logger.LogDebug($"app.AddInteractiveWebAssemblyRenderMode()");
logger.LogDebug($"app.AddAdditionalAssemblies(typeof(SampleBlazorApp.Client._Imports).Assembly);");

app.Run(); logger.LogDebug($"app.Run();");
