using System;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Core.Services.Extensions;

/// <summary>
/// Extension methods for <see cref="ILogService"/>.
/// </summary>
public static class LogServiceExtensions
{
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