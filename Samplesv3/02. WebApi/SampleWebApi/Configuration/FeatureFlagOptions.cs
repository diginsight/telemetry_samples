using Diginsight.CAOptions;
using Diginsight.Diagnostics;

namespace SampleWebApi;

public class FeatureFlagOptions : IDynamicallyConfigurable, IVolatilelyConfigurable
{
    public bool TraceRequestBody { get; set; }
    public bool TraceResponseBody { get; set; }
}
