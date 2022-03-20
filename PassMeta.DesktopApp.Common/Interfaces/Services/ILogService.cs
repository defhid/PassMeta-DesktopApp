namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using Models.Entities;

    /// <summary>
    /// Service for logging user messages and app errors.
    /// </summary>
    public interface ILogService : IDisposable
    {
        /// <summary>
        /// Log information text.
        /// </summary>
        void Info(string text);
        
        /// <summary>
        /// Log warning text.
        /// </summary>
        void Warning(string text);
        
        /// <summary>
        /// Log error text.
        /// </summary>
        void Error(string text);

        /// <summary>
        /// Log error text with exception.
        /// </summary>
        void Error(Exception ex, string? text = null);

        /// <summary>
        /// Get application logs by period.
        /// </summary>
        List<Log> ReadLogs(DateTime dateFrom, DateTime dateTo);

        /// <summary>
        /// Delete old logs without throwing exceptions.
        /// </summary>
        void OptimizeLogs();
    }
}