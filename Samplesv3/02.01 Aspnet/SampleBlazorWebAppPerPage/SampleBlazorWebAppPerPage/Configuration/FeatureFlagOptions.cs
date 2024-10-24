using Diginsight.Options;

namespace SampleBlazorWebAppPerPage;

public class FeatureFlagOptions : IDynamicallyConfigurable, IVolatilelyConfigurable
{
    public bool TraceRequestBody { get; set; }
    public bool TraceResponseBody { get; set; }

}
