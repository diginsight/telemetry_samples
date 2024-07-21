using Diginsight.Diagnostics;
using System.Diagnostics;

namespace BlazorAppClient1
{
    public sealed class BlazorActivityLogEmitter : IActivityListenerLogic
    {

        public BlazorActivityLogEmitter()
        {
        }

        void IActivityListenerLogic.ActivityStarted(Activity activity)
        {
            Console.WriteLine($"{activity.DisplayName} START");
        }

        void IActivityListenerLogic.ActivityStopped(Activity activity)
        {
            Console.WriteLine($"{activity.DisplayName} END");
        }

        ActivitySamplingResult IActivityListenerLogic.Sample(ref ActivityCreationOptions<ActivityContext> creationOptions)
        {
            return ActivitySamplingResult.AllData;
        }
    }
}
