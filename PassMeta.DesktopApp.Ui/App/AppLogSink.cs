using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Extensions;
using Splat;

namespace PassMeta.DesktopApp.Ui.App;

/// <inheritdoc cref="ILogSink" />
public sealed class AppLogSink : ILogSink, IDisposable
{
    private readonly LogEventLevel _minLevel;

    public AppLogSink(LogEventLevel minLevel)
    {
        _minLevel = minLevel;
    }

    #region ILogSink

    /// <inheritdoc />
    public bool IsEnabled(LogEventLevel level, string area) => IsEnabled(level);

    /// <inheritdoc />
    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        if (IsEnabled(level))
            LogInternal(area, source, messageTemplate);
    }

    /// <inheritdoc />
    public void Log<T0>(LogEventLevel level, string area, object? source, string messageTemplate,
        T0 propertyValue0)
    {
        if (IsEnabled(level))
            LogInternal(area, source, messageTemplate, propertyValue0);
    }

    /// <inheritdoc />
    public void Log<T0, T1>(LogEventLevel level, string area, object? source, string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1)
    {
        if (IsEnabled(level))
            LogInternal(area, source, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <inheritdoc />
    public void Log<T0, T1, T2>(LogEventLevel level, string area, object? source, string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2)
    {
        if (IsEnabled(level))
            LogInternal(area, source, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <inheritdoc />
    public void Log(LogEventLevel level, string area, object? source, string messageTemplate,
        params object?[] propertyValues)
    {
        if (IsEnabled(level))
            LogInternal(area, source, messageTemplate, propertyValues);
    }

    #endregion

    /// <inheritdoc />
    public void Dispose() => Locator.Current.ResolveOrDefault<ILogsWriter>()?.Flush();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LogInternal(string area, object? source, string messageTemplate, params object?[] args)
    {
        var message = $"UI, {area}, {source}: {messageTemplate} [{string.Join("; ", args)}]";

#if DEBUG
        if (Debugger.IsAttached) Debugger.Break();
#endif

        Locator.Current.ResolveOrDefault<ILogsWriter>()?.Warning(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsEnabled(LogEventLevel level) => level >= _minLevel;
}