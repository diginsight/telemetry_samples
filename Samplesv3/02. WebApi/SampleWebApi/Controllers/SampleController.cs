using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SampleWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "common")]
    public class SampleController : ControllerBase
    {
        private readonly ILogger<SampleController> logger;
        
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public SampleController(ILogger<SampleController> logger)
        {
            logger = logger;
        }

        [HttpGet("", Name = "Get")]
        //[ApiVersion(ApiVersions.V_2024_04_26.Name)]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet ("dosomework", Name = "DoSomeWork")]
        [ApiVersion(ApiVersions.V_2024_04_26.Name)]
        public async Task DoSomeWork() 
        {
            //using var activity = source.StartMethodActivity(logger, new { foo, bar });

            //var result1 = await StepOne();
            //logger.LogDebug($"await StepOne(); returned {result1}");

            //var result2 = await StepTwo();
            //logger.LogDebug($"await StepTwo(); completed {result2}");

            //var result = result1 + result2;

            //activity.StoreOutput(result);

            return ;
        }
    }
}
