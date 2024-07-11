using Diginsight.CAOptions;

namespace SampleBlazorWebAppGlobal;

public class FeatureFlagOptions : IDynamicallyConfigurable, IVolatilelyConfigurable
{
    public bool TraceRequestBody { get; set; }
    public bool TraceResponseBody { get; set; }

}
