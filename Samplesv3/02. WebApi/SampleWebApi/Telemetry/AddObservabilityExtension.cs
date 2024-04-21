#nullable enable

using Azure.Monitor.OpenTelemetry.Exporter;
using Diginsight;
using Diginsight.AspNetCore;
using Diginsight.CAOptions;
using Diginsight.Diagnostics;
using Diginsight.Diagnostics.AspNetCore;
using Diginsight.Diagnostics.Log4Net;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;

namespace SampleWebApi;

public sealed class OpenTelemetryOptions : IDynamicallyPostConfigurable
{
    public bool EnableTraces { get; set; }
    public bool EnableMetrics { get; set; }

    public double TracingSamplingRatio { get; set; }

    public ICollection<string> ActivitySources { get; } = new List<string>();

    public ICollection<string> ExcludedHttpHosts { get; } = new List<string>();

    public ICollection<string> DurationMetricTags { get; } = new List<string>();

    object IDynamicallyPostConfigurable.MakeFiller() => new Filler(this);

    private sealed class Filler
    {
        private readonly OpenTelemetryOptions filled;

        public bool EnableTraces
        {
            get => filled.EnableTraces;
            set => filled.EnableTraces = value;
        }

        public bool EnableMetrics
        {
            get => filled.EnableMetrics;
            set => filled.EnableMetrics = value;
        }

        public double TracingSamplingRatio
        {
            get => filled.TracingSamplingRatio;
            set => filled.TracingSamplingRatio = value;
        }

        public Filler(OpenTelemetryOptions filled)
        {
            this.filled = filled;
        }
    }
}

public static class AddObservabilityExtension
{
    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

        DiginsightDefaults.ActivitySource = Program.ActivitySource;

        IConfiguration openTelemetryConfiguration = configuration.GetSection("OpenTelemetry");
        OpenTelemetryOptions openTelemetryOptions = new OpenTelemetryOptions();
        openTelemetryConfiguration.Bind(openTelemetryOptions);
        services.Configure<OpenTelemetryOptions>(openTelemetryConfiguration);
        services.PostConfigureFromHttpRequestHeaders<OpenTelemetryOptions>();

        services.TryAddSingleton<IActivityLoggingSampler, HttpHeadersActivityLoggingSampler>();
        services.Decorate<IActivityLoggingSampler, MyActivityLoggingSampler>();

        services.TryAddSingleton<ISpanDurationMetricProcessorSettings, MySpanDurationMetricProcessorSettings>();
        services.TryAddSingleton<ICustomDurationMetricProcessorSettings, MyCustomDurationMetricProcessorSettings>();

        var azureMonitorConnectionString = configuration["ApplicationInsights:ConnectionString"];
        services.AddLogging(
            loggingBuilder =>
            {
                loggingBuilder.ClearProviders();

                if (configuration.GetValue("AppSettings:ConsoleProviderEnabled", true))
                {
                    loggingBuilder.AddDiginsightConsole();
                }

                if (configuration.GetValue("AppSettings:Log4NetProviderEnabled", false))
                {
                    loggingBuilder.AddDiginsightLog4Net("log4net.config");
                }

                if (!string.IsNullOrEmpty(azureMonitorConnectionString))
                {
                    loggingBuilder.AddOpenTelemetry(
                        otlo => otlo.AddAzureMonitorLogExporter(
                            exporterOptions => { exporterOptions.ConnectionString = azureMonitorConnectionString; }
                        )
                    );
                }
            }
        );

        services.ConfigureClassAware<DiginsightActivitiesOptions>(configuration.GetSection("Diginsight:Activities"));
        services.PostConfigureFromHttpRequestHeaders<DiginsightActivitiesOptions>();

        var builder = services.AddDiginsightOpenTelemetry();

        if (openTelemetryOptions.EnableMetrics)
        {
            builder.WithMetrics(
                meterProviderBuilder =>
                {
                    meterProviderBuilder
                        .AddDiginsight()
                        .AddAspNetCoreInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddMeter(DiginsightDefaults.Meter.Name)
                        .AddPrometheusExporter();

                    if (!string.IsNullOrEmpty(azureMonitorConnectionString))
                    {
                        meterProviderBuilder.AddAzureMonitorMetricExporter(
                            exporterOptions => { exporterOptions.ConnectionString = azureMonitorConnectionString; }
                        );
                    }
                }
            );
        }

        if (openTelemetryOptions.EnableTraces)
        {
            builder.WithTracing(
                tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .AddProcessor<ActivitySourceDetectionProcessor>()
                        .AddProcessor<SpanDurationMetricProcessor>()
                        .AddDiginsight()
                        .AddAspNetCoreInstrumentation(
                            static options =>
                            {
                                options.EnrichWithHttpRequest = static (activity, httpRequest) =>
                                {
                                    var context = httpRequest.HttpContext;

                                    activity.DisplayName = $"{context.Request.Method.ToUpperInvariant()} {context.Request.Path}";
                                    activity.AddTag("http.client_ip", context.Connection.RemoteIpAddress);
                                    activity.AddTag("http.request_content_length", httpRequest.ContentLength);
                                    activity.AddTag("http.request_content_type", httpRequest.ContentType);
                                };
                                options.EnrichWithHttpResponse = static (activity, httpResponse) =>
                                {
                                    activity.AddTag("http.response_content_length", httpResponse.ContentLength);
                                    activity.AddTag("http.response_content_type", httpResponse.ContentType);
                                };
                                options.EnrichWithException = static (activity, exception) =>
                                {
                                    activity.SetTag("stack_trace", exception.StackTrace);
                                };
                            }
                        )
                        .AddHttpClientInstrumentation(
                            options =>
                            {
                                options.EnrichWithHttpRequestMessage = static (activity, httpRequestMessage) =>
                                {
                                    if (httpRequestMessage.Content is not null)
                                    {
                                        byte[] contentByteArray = httpRequestMessage.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                                        activity.AddTag("http.request_content", Encoding.UTF8.GetString(contentByteArray));
                                    }
                                };
                                options.EnrichWithHttpResponseMessage = static (activity, httpResponseMessage) =>
                                {
                                    byte[] contentByteArray = httpResponseMessage.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                                    int contentLength = contentByteArray.Length;
                                    activity.AddTag("http.response_content_length", contentLength);
                                    if (!httpResponseMessage.IsSuccessStatusCode && contentLength > 0)
                                    {
                                        activity.AddTag("http.response_content", Encoding.UTF8.GetString(contentByteArray));
                                    }
                                };
                                options.EnrichWithException = static (activity, exception) =>
                                {
                                    //activity.SetTag("stack_trace", exception.StackTrace);
                                    activity.SetTag("error", true);
                                };
                                options.FilterHttpRequestMessage = httpRequestMessage =>
                                {
                                    string requestHost = httpRequestMessage.RequestUri!.Host;

                                    foreach (string excludedHost in openTelemetryOptions.ExcludedHttpHosts)
                                    {
                                        if (excludedHost[0] == '.' && requestHost.EndsWith(excludedHost, StringComparison.Ordinal))
                                            return false;
                                        if (requestHost == excludedHost)
                                            return false;
                                    }

                                    return true;
                                };
                            }
                        )
                        .AddSource(openTelemetryOptions.ActivitySources.ToArray())
                        .AddSource(Program.ActivitySource.Name)
                        .SetErrorStatusOnException()
                        .SetSampler(
                            static sp =>
                            {
                                OpenTelemetryOptions openTelemetryOptions = sp.GetRequiredService<IOptions<OpenTelemetryOptions>>().Value;
                                Sampler decoratee = new ParentBasedSampler(new TraceIdRatioBasedSampler(openTelemetryOptions.TracingSamplingRatio));
                                return ActivatorUtilities.CreateInstance<HttpHeadersSampler>(sp, decoratee);
                            }
                        );

                    if (!string.IsNullOrEmpty(azureMonitorConnectionString))
                    {
                        tracerProviderBuilder.AddAzureMonitorTraceExporter(
                            exporterOptions => { exporterOptions.ConnectionString = azureMonitorConnectionString; }
                        );
                    }
                }
            );
        }

        return services;
    }

    private static IEnumerable<KeyValuePair<string, object?>> CoreExtractTags(Activity activity, OpenTelemetryOptions openTelemetryOptions)
    {
        return openTelemetryOptions.DurationMetricTags
            .Select(k => (Key: k, Value: activity.GetAncestors(true).Select(a => a.GetTagItem(k)).FirstOrDefault(static v => v is not null)))
            .Where(static x => x.Value is not null)
            .Select(static x => KeyValuePair.Create(x.Key, x.Value));
    }

    private sealed class MyActivityLoggingSampler : IActivityLoggingSampler
    {
        private readonly IActivityLoggingSampler decoratee;

        public MyActivityLoggingSampler(IActivityLoggingSampler decoratee)
        {
            this.decoratee = decoratee;
        }

        public bool? ShouldLog(Activity activity)
        {
            return activity is { OperationName: "System.Net.Http.HttpRequestOut", Source.Name: "System.Net.Http" }
                ? false
                : decoratee.ShouldLog(activity);
        }
    }

    private sealed class MySpanDurationMetricProcessorSettings : DefaultSpanDurationMetricProcessorSettings
    {
        private readonly IClassAwareOptionsMonitor<OpenTelemetryOptions> openTelemetryOptionsMonitor;

        public MySpanDurationMetricProcessorSettings(IClassAwareOptionsMonitor<OpenTelemetryOptions> openTelemetryOptionsMonitor)
        {
            this.openTelemetryOptionsMonitor = openTelemetryOptionsMonitor;
        }

        public override IEnumerable<KeyValuePair<string, object?>> ExtractTags(Activity activity)
        {
            OpenTelemetryOptions openTelemetryOptions = openTelemetryOptionsMonitor.Get(activity.GetCallerType());
            return base.ExtractTags(activity).Concat(CoreExtractTags(activity, openTelemetryOptions));
        }
    }

    private sealed class MyCustomDurationMetricProcessorSettings : ICustomDurationMetricProcessorSettings
    {
        private readonly IClassAwareOptionsMonitor<OpenTelemetryOptions> openTelemetryOptionsMonitor;

        public MyCustomDurationMetricProcessorSettings(IClassAwareOptionsMonitor<OpenTelemetryOptions> openTelemetryOptionsMonitor)
        {
            this.openTelemetryOptionsMonitor = openTelemetryOptionsMonitor;
        }

        public bool? ShouldRecord(Activity activity, Instrument instrument) => null;

        public IEnumerable<KeyValuePair<string, object?>> ExtractTags(Activity activity, Instrument instrument)
        {
            OpenTelemetryOptions openTelemetryOptions = openTelemetryOptionsMonitor.Get(activity.GetCallerType());
            return CoreExtractTags(activity, openTelemetryOptions);
        }
    }
}
