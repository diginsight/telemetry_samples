using System.Diagnostics;
using Diginsight.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace SampleBlazorWebApp.Client.Pages;

public class CounterBase : ComponentBase
{
    protected int currentCount = 0;

    [Inject] private ILogger<CounterBase> logger { get; set; } = null!;

    protected void IncrementCount()
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);

        //var process = Process.GetCurrentProcess();
        //logger.LogDebug("process: {ProcessId}, {ProcessName}", process.Id, process.ProcessName);

        currentCount++;
    }
}   
