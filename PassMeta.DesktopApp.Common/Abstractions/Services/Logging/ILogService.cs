namespace PassMeta.DesktopApp.Common.Abstractions.Services.Logging;

using System;
using System.Collections.Generic;
using Extra;
using PassMeta.DesktopApp.Common.Models.Entities;

/// <summary>
/// Service for logging user messages and app errors.
/// </summary>
public interface ILogService : IDisposable
{
    /// <summary>
    /// Write application log.
    /// </summary>
    void Write(Log log);

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
    event EventHandler<LoggerErrorEventArgs> ErrorOccured;
}