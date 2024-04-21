using OpenTelemetry;
using System.Diagnostics;

namespace SampleWebApi;

public class ActivitySourceDetectionProcessor : BaseProcessor<Activity>
{
    private readonly ILogger logger;
    private readonly ISet<string> seenActivitySources = new HashSet<string>();

    public ActivitySourceDetectionProcessor(ILogger<ActivitySourceDetectionProcessor> logger)
    {
        this.logger = logger;
    }

    public override void OnStart(Activity activity)
    {
        string activitySourceName = activity.Source.Name;
        if (seenActivitySources.Add(activitySourceName))
        {
            logger.LogDebug("New activity source detected: {ActivitySource}", activitySourceName);
        }
    }
}
