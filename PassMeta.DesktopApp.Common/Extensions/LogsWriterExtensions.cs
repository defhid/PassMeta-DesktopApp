using System;
using PassMeta.DesktopApp.Common.Abstractions.AppConfig;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="ILogsWriter"/>.
/// </summary>
public static class LogsWriterExtensions
{
    /// <summary>
    /// Log debug text, if logger config
    /// has <see cref="IAppConfig.DebugMode"/> flag.
    /// </summary>
    public static void Debug(this ILogsWriter logsWriter, string text)
    {
        if (logsWriter.AppConfigProvider?.Current.DebugMode is true)
        {
            logsWriter.Write(new Log { Section = Log.Sections.Debug, Text = text });
        }
    }

    /// <summary>
    /// Log debug text, if logger config
    /// has <see cref="IAppConfig.DebugMode"/> flag.
    /// </summary>
    public static void Debug(this ILogsWriter logsWriter, string formattedText, params object[] args)
    {
        if (logsWriter.AppConfigProvider?.Current.DebugMode is true)
        {
            logsWriter.Write(new Log { Section = Log.Sections.Debug, Text = string.Format(formattedText, args) });
        }
    }

    /// <summary>
    /// Log information text.
    /// </summary>
    public static void Info(this ILogsWriter logsWriter, string text)
        => logsWriter.Write(new Log { Section = Log.Sections.Info, Text = text });

    /// <summary>
    /// Log warning text.
    /// </summary>
    public static void Warning(this ILogsWriter logsWriter, string text)
        => logsWriter.Write(new Log { Section = Log.Sections.Warning, Text = text });

    /// <summary>
    /// Log error text.
    /// </summary>
    public static void Error(this ILogsWriter logsWriter, string text)
        => logsWriter.Write(new Log { Section = Log.Sections.Error, Text = text });

    /// <summary>
    /// Log error text with exception.
    /// </summary>
    public static void Error(this ILogsWriter logsWriter, Exception ex, string? text = null)
        => logsWriter.Error(text is null ? ex.ToString() : text + $" [{ex}]");
}