using Diginsight.AspNetCore;

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

            services.AddControllers();

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
