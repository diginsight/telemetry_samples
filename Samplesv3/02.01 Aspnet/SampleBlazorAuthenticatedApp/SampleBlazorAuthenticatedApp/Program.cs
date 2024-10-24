using Diginsight;
using Diginsight.AspNetCore;
using Diginsight.Diagnostics;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RestSharp;
using SampleBlazorAuthenticatedApp;
using SampleBlazorAuthenticatedApp.Client.Pages;
using SampleBlazorAuthenticatedApp.Components;
using SampleBlazorAuthenticatedApp.Components.Account;
using SampleBlazorAuthenticatedApp.Data;
using System.Text.Json.Serialization;

namespace SampleBlazorAuthenticatedApp;

public class Program
{
    public static IDeferredLoggerFactory DeferredLoggerFactory;

    public static void Main(string[] args)
    {
        var activitiesOptions = new DiginsightActivitiesOptions() { LogActivities = true };
        DeferredLoggerFactory = new DeferredLoggerFactory(activitiesOptions: activitiesOptions);
        //DeferredLoggerFactory.ActivitySources.Add(Observability.ActivitySource);
        DeferredLoggerFactory.ActivitySourceFilter = (activitySource) => activitySource.Name.StartsWith($"Sample");
        var logger = DeferredLoggerFactory.CreateLogger<Program>();

        var app = default(WebApplication);
        using (var activity = Observability.ActivitySource.StartMethodActivity(logger, new { args }))
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.TryAddSingleton(DeferredLoggerFactory);
            ConfigureServices(builder.Services, builder.Configuration);

            var webHost = builder.Host.UseDiginsightServiceProvider();
            app = builder.Build();

            Configure(app, app.Environment);

            logger.LogDebug("Host built");
        }

        app.Run();
    }


    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var logger = DeferredLoggerFactory.CreateLogger<Program>();
        using var innerActivity = Observability.ActivitySource.StartMethodActivity(logger, new { services });

        services.FlushOnCreateServiceProvider(DeferredLoggerFactory);
        services.AddHttpContextAccessor();
        services.AddVolatileConfiguration();
        services.AddObservability(configuration);
        services.AddDynamicLogLevel<DefaultDynamicLogLevelInjector>();

        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        services.ConfigureClassAware<FeatureFlagOptions>(configuration.GetSection("FeatureManagement"))
            .DynamicallyConfigureClassAwareFromHttpRequestHeaders<FeatureFlagOptions>();

        //static void ConfigureTypeContracts(LogStringTypeContractAccessor accessor) // configure type contracts for log string rendering
        //{
        //    accessor.GetOrAdd<RestResponse>(
        //        static typeContract =>
        //        {
        //            typeContract.GetOrAdd(static x => x.Request, static mc => mc.Included = false);
        //            typeContract.GetOrAdd(static x => x.ResponseStatus, static mc => mc.Order = 1);
        //            //typeContract.GetOrAdd(static x => x.Content, static mc => mc.Order = 1);
        //        }
        //    );
        //}
        //AppendingContextFactoryBuilder.DefaultBuilder.ConfigureContracts(ConfigureTypeContracts);
        //services.Configure<LogStringTypeContractAccessor>(ConfigureTypeContracts);

        services.AddApiVersioning(opt =>
        {
            opt.DefaultApiVersion = ApiVersions.V_2024_04_26.Version;
            opt.AssumeDefaultVersionWhenUnspecified = true;

            // ToDo: add error response (opt.ErrorResponses)
        });

        services.AddControllers()
            .AddControllersAsServices()
            .ConfigureApiBehaviorOptions(opt =>
            {
                opt.SuppressModelStateInvalidFilter = true;
            })
            .AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                opt.JsonSerializerOptions.WriteIndented = true;

                //opt.JsonSerializerOptions.PropertyNamingPolicy = new PascalCaseJsonNamingPolicy();
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .AddMvcOptions(opt =>
            {
                opt.MaxModelValidationErrors = 25;
                //opt.Conventions.Add(new DataExportConvention() as IControllerModelConvention);
                //opt.Conventions.Add(new DataExportConvention() as IActionModelConvention);
            });

        // Add services to the container.
        services.AddCascadingAuthenticationState();
        services.AddScoped<IdentityUserAccessor>();
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

        var isSwaggerEnabled = configuration.GetValue<bool>("IsSwaggerEnabled");
        if (isSwaggerEnabled)
        {
            services.AddSwaggerDocumentation();
        }

    }


    public static void Configure(WebApplication app, IWebHostEnvironment env)
    {
        var logger = DeferredLoggerFactory.CreateLogger<Program>();
        using var innerActivity = Observability.ActivitySource.StartMethodActivity(logger, new { app, env });

        var configuration = app.Services.GetRequiredService<IConfiguration>();

        //// Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            //IdentityModelEventSource.ShowPII = true;
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        //app.UseOpenTelemetryPrometheusScrapingEndpoint();
        app.UseRouting();
        app.UseCors();

        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(SampleBlazorAuthenticatedApp.Client._Imports).Assembly);

        //// Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        var isSwaggerEnabled = configuration.GetValue<bool>("IsSwaggerEnabled");
        if (isSwaggerEnabled)
        {
            app.UseSwaggerDocumentation();
        }

    }
}
