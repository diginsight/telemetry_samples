using Diginsight;
using Diginsight.AspNetCore;
using Diginsight.Strings;
using RestSharp;
using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Text.Json.Serialization;
using Diginsight.Diagnostics;

namespace SampleWebApi
{
    public class Startup
    {
        private static readonly string SmartCacheServiceBusSubscriptionName = Guid.NewGuid().ToString("N");

        private readonly IDeferredLoggerFactory deferredLoggerFactory;
        private readonly ILogger logger;
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration, IDeferredLoggerFactory deferredLoggerFactory)
        {
            this.configuration = configuration;

            this.deferredLoggerFactory = deferredLoggerFactory;
            this.logger = this.deferredLoggerFactory.CreateLogger<Startup>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddObservability(configuration);
            services.AddDynamicLogLevel<DefaultDynamicLogLevelInjector>();
            services.FlushOnCreateServiceProvider(deferredLoggerFactory);

            services.ConfigureClassAware<FeatureFlagOptions>(configuration.GetSection("FeatureManagement"))
                .PostConfigureClassAwareFromHttpRequestHeaders<FeatureFlagOptions>();

            // configure type contracts for log string rendering
            static void ConfigureTypeContracts(LogStringTypeContractAccessor accessor)
            {
                accessor.GetOrAdd<RestResponse>(
                    static typeContract =>
                    {
                        typeContract.GetOrAdd(static x => x.Request, static mc => mc.Included = false);
                        typeContract.GetOrAdd(static x => x.ResponseStatus, static mc => mc.Order = 1);
                        //typeContract.GetOrAdd(static x => x.Content, static mc => mc.Order = 1);
                    }
                );
            }
            AppendingContextFactoryBuilder.DefaultBuilder.ConfigureContracts(ConfigureTypeContracts);
            services.Configure<LogStringTypeContractAccessor>(ConfigureTypeContracts);

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

            IsSwaggerEnabled = configuration.GetValue<bool>("IsSwaggerEnabled");
            if (IsSwaggerEnabled)
            {
                services.AddSwaggerDocumentation();
            }
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseOpenTelemetryPrometheusScrapingEndpoint();

            if (IsSwaggerEnabled)
            {
                app.UseSwaggerDocumentation();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
        private bool IsSwaggerEnabled { get; set; }

    }
}
