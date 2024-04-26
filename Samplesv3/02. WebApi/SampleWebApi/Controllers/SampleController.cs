using Asp.Versioning;
using Diginsight.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

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
            this.logger = logger;
        }

        [HttpGet("", Name = "Get")]
        [ApiVersion(ApiVersions.V_2024_04_26.Name)]
        public IEnumerable<WeatherForecast> Get()
        {
            using var activity = Program.ActivitySource.StartMethodActivity(logger); // , new { foo, bar }

            var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            activity.SetOutput(result);
            return result;
        }

        [HttpGet("dosomework", Name = "DoSomeWork")]
        [ApiVersion(ApiVersions.V_2024_04_26.Name)]
        public async Task DoSomeWork()
        {
            using var activity = Program.ActivitySource.StartMethodActivity(logger); // , new { foo, bar }

            var result1 = await StepOne();
            logger.LogDebug("await StepOne(); returned {result1}", result1);

            var result2 = await StepTwo();
            logger.LogDebug("await StepTwo(); completed {result2}", result2);

            var result = result1 + result2;

            activity.SetOutput(result);
            return;
        }

        private async Task<int> StepOne()
        {
            using var activity = Program.ActivitySource.StartMethodActivity(logger); // , new { foo, bar }

            var result = 1; 
            activity.SetOutput(result);
            return result;
        }
        private async Task<int> StepTwo()
        {
            using var activity = Program.ActivitySource.StartMethodActivity(logger); // , new { foo, bar }

            var result = 1;
            activity.SetOutput(result);
            return result;
        }
    }
}
