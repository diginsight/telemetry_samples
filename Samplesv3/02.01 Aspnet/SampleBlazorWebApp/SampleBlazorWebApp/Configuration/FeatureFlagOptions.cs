﻿using Diginsight.CAOptions;

namespace SampleBlazorWebApp;

public class FeatureFlagOptions : IDynamicallyPostConfigurable
{
    public bool TraceRequestBody { get; set; }
    public bool TraceResponseBody { get; set; }

}
