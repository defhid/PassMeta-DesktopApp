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
    public void Log(LogEventLevel level, string area, object? source, string messageTemplate,
        params object?[] propertyValues)
    {
        if (IsEnabled(level))
            LogInternal(area, source, messageTemplate, propertyValues);
    }

    #endregion

    /// <inheritdoc />
    public void Dispose() => Locator.Current.ResolveOrDefault<ILogsWriter>()?.Flush();

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