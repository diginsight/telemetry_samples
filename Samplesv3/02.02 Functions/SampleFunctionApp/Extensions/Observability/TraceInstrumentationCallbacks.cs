using System.Diagnostics;

namespace SampleFunctionApp;

public sealed class TraceInstrumentationCallbacks
{
    public Func<Activity, HttpRequestMessage, bool> ShouldTagWithRequestContent { get; set; } = static (_, _) => false;

    public Func<Activity, HttpResponseMessage, bool> ShouldTagWithResponseContent { get; set; } = static (_, _) => false;

    public Func<Activity, Exception, bool> ShouldTagWithStackTrace { get; set; } = static (_, _) => true;
}
