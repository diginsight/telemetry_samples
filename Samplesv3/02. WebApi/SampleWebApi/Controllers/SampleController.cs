using Asp.Versioning;
using Diginsight.CAOptions;
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
        private readonly IClassAwareOptionsMonitor<FeatureFlagOptions> featureFlagsOptionsMonitor;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public SampleController(ILogger<SampleController> logger,
            IClassAwareOptionsMonitor<FeatureFlagOptions> featureFlagsOptionsMonitor)
        {
            this.logger = logger;
            this.featureFlagsOptionsMonitor = featureFlagsOptionsMonitor;
        }

        [HttpGet("", Name = "Get")]
        [ApiVersion(ApiVersions.V_2024_04_26.Name)]
        public IEnumerable<WeatherForecast> Get()
        {
            using var activity = Program.ActivitySource.StartMethodActivity(logger); // , new { foo, bar }

            var result = default(IEnumerable<WeatherForecast>);

            var permissionCheckEnabled = featureFlagsOptionsMonitor.CurrentValue.PermissionCheckEnabled;
            var traceRequestBody = featureFlagsOptionsMonitor.CurrentValue.TraceRequestBody;
            var traceResponseBody = featureFlagsOptionsMonitor.CurrentValue.TraceResponseBody;

            logger.LogDebug("PermissionCheckEnabled: {PermissionCheckEnabled}", permissionCheckEnabled);
            logger.LogDebug("TraceRequestBody: {TraceRequestBody}", traceRequestBody);
            logger.LogDebug("TraceResponseBody: {TraceResponseBody}", traceResponseBody);

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
