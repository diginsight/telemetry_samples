using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
//using Diginsight.Diagnostics;

namespace SampleFunctionApp1
{
    public class Function1
    {


        [FunctionName("Function1")]
        public void Run([TimerTrigger("0/30 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            //using var activity = Observability.ActivitySource.StartMethodActivity(log, new { myTimer });

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }

}
