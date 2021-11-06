namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.Entities;

    /// <summary>
    /// Service for logging user messages & app errors.
    /// </summary>
    public interface ILogService : IDisposable
    {
        void Info(string text);
        
        void Warning(string text);
        
        void Error(string text);

        void Error(Exception ex, string? text = null);

        // TODO: UI
        Task<List<Log>> ReadLogsAsync(DateTime dateFrom, DateTime dateTo);
    }
}