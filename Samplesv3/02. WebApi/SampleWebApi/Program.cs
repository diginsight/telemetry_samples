//using Microsoft.AspNetCore;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();

using Diginsight.Diagnostics.AspNetCore;
using Microsoft.AspNetCore;
using System.Diagnostics;
namespace SampleWebApi;

public class Program
{
    internal static readonly ActivitySource ActivitySource = new(typeof(Program).Namespace ?? typeof(Program).Name!);

    public static void Main(string[] args)
    {
        //DiginsightActivitiesOptions activitiesOptions = new ()
        //{
        //    LogActivities = true,
        //};
        //IDeferredLoggerFactory loggerFactory = new DeferredLoggerFactory(activitiesOptions: activitiesOptions);
        //ILogger logger = loggerFactory.CreateLogger<Program>();
        //ActivitySource deferredActivitySource = loggerFactory.ActivitySource;

        IWebHost host;
        //using (deferredActivitySource.StartMethodActivity(logger))
        {
            host = WebHost.CreateDefaultBuilder(args)
                .AddKeyVault()
                .UseStartup<Startup>()
                //.ConfigureServices(s => s.FlushOnCreateServiceProvider(loggerFactory))
                .UseDiginsightServiceProvider()
                .Build();

            //logger.LogDebug("Host built");
        }

        host.Run();
    }
}