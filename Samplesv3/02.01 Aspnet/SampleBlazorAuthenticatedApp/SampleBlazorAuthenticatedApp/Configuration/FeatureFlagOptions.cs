using Diginsight.Options;

namespace SampleBlazorAuthenticatedApp;

public class FeatureFlagOptions : IDynamicallyConfigurable, IVolatilelyConfigurable
{
    public bool TraceRequestBody { get; set; }
    public bool TraceResponseBody { get; set; }

}
