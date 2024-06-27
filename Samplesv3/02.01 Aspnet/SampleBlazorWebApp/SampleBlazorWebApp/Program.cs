using SampleBlazorWebApp.Client.Pages;
using SampleBlazorWebApp.Components;
using Diginsight.CAOptions;
using Diginsight.Diagnostics;

namespace SampleBlazorWebApp;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Diginsight.Diagnostics.AspNetCore;

public class Program
{
    public static IDeferredLoggerFactory DeferredLoggerFactory;

    public static void Main(string[] args)
    {
        var activitiesOptions = new DiginsightActivitiesOptions() { LogActivities = true };
        DeferredLoggerFactory = new DeferredLoggerFactory(activitiesOptions: activitiesOptions);
        DeferredLoggerFactory.ActivitySources.Add(Observability.ActivitySource);
        var logger = DeferredLoggerFactory.CreateLogger<Program>();

        IWebHost host;
        using (var activity = Observability.ActivitySource.StartMethodActivity(logger, new { args }))
        {
            host = WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration2()
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    var logger = DeferredLoggerFactory.CreateLogger<Startup>();
                    using var innerActivity = Observability.ActivitySource.StartRichActivity(logger, "ConfigureServicesCallback", new { services });

                    services.TryAddSingleton(DeferredLoggerFactory);
                })
                .UseDiginsightServiceProvider()
                .Build();

            logger.LogDebug("Host built");
        }

        host.Run();


        //var builder = WebApplication.CreateBuilder(args);

        //// Add services to the container.
        //builder.Services.AddRazorComponents()
        //    .AddInteractiveServerComponents()
        //    .AddInteractiveWebAssemblyComponents();

        //var app = builder.Build();

        //// Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
        //    app.UseWebAssemblyDebugging();
        //}
        //else
        //{
        //    app.UseExceptionHandler("/Error", createScopeForErrors: true);
        //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        //    app.UseHsts();
        //}

        //app.UseHttpsRedirection();

        //app.UseStaticFiles();
        //app.UseAntiforgery();

        //app.MapRazorComponents<App>()
        //    .AddInteractiveServerRenderMode()
        //    .AddInteractiveWebAssemblyRenderMode()
        //    .AddAdditionalAssemblies(typeof(SampleBlazorWebApp.Client._Imports).Assembly);

        //app.Run();


    }
}
