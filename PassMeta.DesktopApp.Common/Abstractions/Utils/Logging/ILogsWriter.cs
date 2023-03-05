using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;

/// <summary>
/// Service for logging user messages and application events.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface ILogsWriter
{
    /// <inheritdoc cref="IAppConfigProvider"/>
    IAppConfigProvider? AppConfigProvider { get; set; }

    /// <summary>
    /// Write application log.
    /// </summary>
    void Write(Log log);
}