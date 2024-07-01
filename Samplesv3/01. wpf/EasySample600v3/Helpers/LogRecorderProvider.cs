using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Diginsight.Diagnostics;

namespace EasySample600v3;


[UnsupportedOSPlatform("browser")]
[ProviderAlias("LogRecorder")]
public sealed class LogRecorderProvider : ILoggerProvider
{
    private readonly IDisposable? _onChangeToken;
    private LogRecorderConfiguration _currentConfig;
    private IDeferredLoggerFactory loggerFactory;
    private readonly ConcurrentDictionary<string, LogRecorder> _loggers = new(StringComparer.OrdinalIgnoreCase);

    public LogRecorderProvider(IOptionsMonitor<LogRecorderConfiguration> config, IDeferredLoggerFactory loggerFactory)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        this.loggerFactory = loggerFactory;
    }

    public ILogger CreateLogger(string categoryName)
    {
        var logger = _loggers.GetOrAdd(categoryName, name => new LogRecorder(name, _currentConfig)); 
        return logger;
    }

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }
}

public sealed class LogRecorderConfiguration
{
    public int EventId { get; set; }

}

public static class LogRecorderExtensions
{
    public static ILoggingBuilder AddLogRecorder(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LogRecorderProvider>());

        LoggerProviderOptions.RegisterProviderOptions<LogRecorderConfiguration, LogRecorderProvider>(builder.Services);

        return builder;
    }

    public static ILoggingBuilder AddLogRecorder(this ILoggingBuilder builder, Action<LogRecorderConfiguration> configure)
    {
        builder.AddLogRecorder();
        builder.Services.Configure(configure);

        return builder;
    }
}

