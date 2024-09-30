
using Diginsight;
using Diginsight.Diagnostics;
using SampleWebApi01;
using SampleWebApi01.Configuration;
using System.Configuration;

public class Program
{
    public static ILoggerFactory LoggerFactory = null;

    static Program()
    {
        var activitiesOptions = new DiginsightActivitiesOptions() { LogActivities = true };
        var deferredLoggerFactory = new DeferredLoggerFactory(activitiesOptions: activitiesOptions);
        deferredLoggerFactory.ActivitySourceFilter = (activitySource) => activitySource.Name.StartsWith($"Sample");
        LoggerFactory = deferredLoggerFactory;
    }

    static void Main(string[] args)
    {
        ILogger logger = LoggerFactory.CreateLogger(typeof(Program));

        var app = default(WebApplication);
        using (var activity = Observability.ActivitySource.StartMethodActivity(logger, new { args }))
        {
            var builder = WebApplication.CreateBuilder(args); logger.LogDebug("builder = WebApplication.CreateBuilder(args);");
            //builder.Host.ConfigureAppConfigurationNH(); logger.LogDebug("builder.Host.ConfigureAppConfigurationNH();");
            builder.Services.AddObservability(builder.Configuration);                               // Diginsight: registers loggers
            builder.Services.FlushOnCreateServiceProvider((IDeferredLoggerFactory)LoggerFactory);   // Diginsight: registers startup log flush
            var webHost = builder.Host.UseDiginsightServiceProvider();                              // Diginsight: Flushes startup log and initializes standard log
            
            logger.LogDebug("builder.Services.AddObservability(builder.Configuration);");
            logger.LogDebug("builder.Services.FlushOnCreateServiceProvider(deferredLoggerFactory);");
            logger.LogDebug("var webHost = builder.Host.UseDiginsightServiceProvider();");

            builder.Services.AddControllers(); logger.LogDebug("builder.Services.AddControllers();");
            builder.Services.AddEndpointsApiExplorer(); logger.LogDebug("builder.Services.AddEndpointsApiExplorer();");
            builder.Services.AddSwaggerGen(); logger.LogDebug("builder.Services.AddSwaggerGen();");

            //builder.Services.ConfigureClassAware<FeatureFlagOptions>(builder.Configuration.GetSection("AppSettings"));

            app = builder.Build(); logger.LogDebug("var app = builder.Build();");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(); logger.LogDebug("app.UseSwagger();");
                app.UseSwaggerUI(); logger.LogDebug("app.UseSwaggerUI();");
            }

            app.UseHttpsRedirection(); logger.LogDebug("app.UseHttpsRedirection();");

            app.UseAuthorization(); logger.LogDebug("app.UseAuthorization();");

            app.MapControllers(); logger.LogDebug("app.MapControllers();");

            app.Run(); logger.LogDebug("app.Run();");
        }
    }
}

