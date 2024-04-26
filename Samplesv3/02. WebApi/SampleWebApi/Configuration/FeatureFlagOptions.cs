using Diginsight.CAOptions;

namespace SampleWebApi;

public class FeatureFlagOptions : IDynamicallyPostConfigurable
{
    public bool PermissionCheckEnabled { get; set; } = true;
    public bool TraceRequestBody { get; set; }
    public bool TraceResponseBody { get; set; }

}
