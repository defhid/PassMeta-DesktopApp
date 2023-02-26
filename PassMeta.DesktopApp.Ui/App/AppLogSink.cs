using Avalonia.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Core.Extensions;
using Splat;

namespace PassMeta.DesktopApp.Ui.App;

/// <inheritdoc />
public class AppLogSink : ILogSink
{
    #region ILogSink

    /// <inheritdoc />
    public bool IsEnabled(LogEventLevel level, string area) => level > LogEventLevel.Information;
    
    /// <inheritdoc />
    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        if (IsEnabled(level, area))
            LogInternal(area, source, messageTemplate);
    }

    /// <inheritdoc />
    public void Log<T0>(LogEventLevel level, string area, object? source, string messageTemplate, T0 propertyValue0)
    {
        if (IsEnabled(level, area))
            LogInternal(area, source, messageTemplate, propertyValue0);
    }

    /// <inheritdoc />
    public void Log<T0, T1>(LogEventLevel level, string area, object? source, string messageTemplate, T0 propertyValue0,
        T1 propertyValue1)
    {
        if (IsEnabled(level, area))
            LogInternal(area, source, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <inheritdoc />
    public void Log<T0, T1, T2>(LogEventLevel level, string area, object? source, string messageTemplate, T0 propertyValue0,
        T1 propertyValue1, T2 propertyValue2)
    {
        if (IsEnabled(level, area))
            LogInternal(area, source, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <inheritdoc />
    public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        if (IsEnabled(level, area))
            LogInternal(area, source, messageTemplate, propertyValues);
    }

    #endregion

    private static void LogInternal(string area, object? source, string messageTemplate, params object?[] args)
    {
        var message = $"{messageTemplate} [{string.Join("; ", args)}]";
        Locator.Current.ResolveOrDefault<ILogsWriter>()?.Warning(message);
    }
}