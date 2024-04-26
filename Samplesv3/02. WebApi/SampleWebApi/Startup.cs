using Diginsight;
using Diginsight.AspNetCore;
using Diginsight.Strings;
using RestSharp;
using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace SampleWebApi
{
    public class Startup
    {
        private static readonly string SmartCacheServiceBusSubscriptionName = Guid.NewGuid().ToString("N");

        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddObservability(configuration);
            services.AddDynamicLogLevel<DefaultDynamicLogLevelInjector>();

            services.ConfigureClassAware<FeatureFlagOptions>(configuration.GetSection("AppSettings"))
                .PostConfigureClassAwareFromHttpRequestHeaders<FeatureFlagOptions>();

            services.AddControllers();

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

            IsSwaggerEnabled = configuration.GetValue<bool>("IsSwaggerEnabled");
            if (IsSwaggerEnabled)
            {
                services.AddSwaggerDocumentation();
            }
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (IsSwaggerEnabled)
            {
                app.UseSwaggerDocumentation();
            }

        }
        private bool IsSwaggerEnabled { get; set; }

    }
}
