using System;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Core.Services.Extensions;

/// <summary>
/// Extension methods for <see cref="ILogService"/>.
/// </summary>
public static class LogServiceExtensions
{
    /// <summary>
    /// Log debug text, if <see cref="AppConfig.Current"/> config
    /// has <see cref="IAppConfig.DebugMode"/> flag.
    /// </summary>
    public static void Debug(this ILogService logService, string text)
    {
        if (AppConfig.Current.DebugMode)
        {
            logService.Write(new Log { Section = Log.Sections.Debug, Text = text });
        }
    }

    /// <summary>
    /// Log debug text, if <see cref="AppConfig.Current"/> config
    /// has <see cref="IAppConfig.DebugMode"/> flag.
    /// </summary>
    public static void Debug(this ILogService logService, string formattedText, params object[] args)
    {
        if (AppConfig.Current.DebugMode)
        {
            logService.Write(new Log { Section = Log.Sections.Debug, Text = string.Format(formattedText, args) });
        }
    }

    /// <summary>
    /// Log information text.
    /// </summary>
    public static void Info(this ILogService logService, string text)
        => logService.Write(new Log { Section = Log.Sections.Info, Text = text });

    /// <summary>
    /// Log warning text.
    /// </summary>
    public static void Warning(this ILogService logService, string text)
        => logService.Write(new Log { Section = Log.Sections.Warning, Text = text });

    /// <summary>
    /// Log error text.
    /// </summary>
    public static void Error(this ILogService logService, string text)
        => logService.Write(new Log { Section = Log.Sections.Error, Text = text });

    /// <summary>
    /// Log error text with exception.
    /// </summary>
    public static void Error(this ILogService logService, Exception ex, string? text = null)
        => logService.Error(text is null ? ex.ToString() : text + $" [{ex}]");
}