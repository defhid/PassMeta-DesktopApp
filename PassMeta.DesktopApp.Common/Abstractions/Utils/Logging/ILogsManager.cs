using System;
using System.Collections.Generic;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging.Extra;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;

/// <summary>
/// Application logs manager.
/// </summary>
public interface ILogsManager : ILogsWriter, IDisposable
{
    /// <summary>
    /// Get application logs by period.
    /// </summary>
    List<Log> Read(DateTime dateFrom, DateTime dateTo);

    /// <summary>
    /// Delete old logs without throwing exceptions.
    /// </summary>
    void CleanUp();

    /// <summary>
    /// The event that is emitted when an error occurs during logging.
    /// </summary>
    event EventHandler<LoggerErrorEventArgs> InternalErrorOccured;
}