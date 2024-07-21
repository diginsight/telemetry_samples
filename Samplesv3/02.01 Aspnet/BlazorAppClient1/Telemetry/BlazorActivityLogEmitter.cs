using Diginsight.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System.Diagnostics;

namespace BlazorAppClient1
{
    internal static class ActivityCustomPropertyNames
    {
        public const string CallerType = nameof(CallerType);
        public const string CustomDurationMetric = nameof(CustomDurationMetric);
        public const string IsStandalone = nameof(IsStandalone);
        public const string Logger = nameof(Logger);
        public const string LogLevel = nameof(LogLevel);
        public const string MakeInputs = nameof(MakeInputs);
        public const string Output = nameof(Output);
        public const string NamedOutputs = nameof(NamedOutputs);
    }

    public sealed class BlazorActivityLogEmitter : IActivityListenerLogic
    {
        private readonly DiginsightActivitiesOptions? activitiesOptions;

        public BlazorActivityLogEmitter(
            ILoggerFactory loggerFactory = null,
            DiginsightActivitiesOptions? activitiesOptions = null)
        {
            this.activitiesOptions = activitiesOptions;
        }

        void IActivityListenerLogic.ActivityStarted(Activity activity)
        {
            ILogger? providedLogger = (ILogger?)activity.GetCustomProperty(ActivityCustomPropertyNames.Logger);
            var logger = providedLogger; // ?? loggerFactory.CreateLogger(callerType);

            if (logger != null) { logger.LogDebug($"{activity.DisplayName} START"); }
            Console.WriteLine($"{activity.DisplayName} START");
        }

        void IActivityListenerLogic.ActivityStopped(Activity activity)
        {
            ILogger? providedLogger = (ILogger?)activity.GetCustomProperty(ActivityCustomPropertyNames.Logger);
            var logger = providedLogger; // ?? loggerFactory.CreateLogger(callerType);

            if (logger != null) { logger.LogDebug($"{activity.DisplayName} END"); }
            Console.WriteLine($"{activity.DisplayName} END");
        }

        ActivitySamplingResult IActivityListenerLogic.Sample(ref ActivityCreationOptions<ActivityContext> creationOptions)
        {
            return ActivitySamplingResult.AllData;
        }
    }
}
