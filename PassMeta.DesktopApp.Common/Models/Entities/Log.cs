namespace PassMeta.DesktopApp.Common.Models.Entities
{
    using System;
    using Interfaces.Services;

    /// <summary>
    /// Log object for <see cref="ILogService"/>.
    /// </summary>
    public class Log
    {
        public string? Section { get; set; }
        
        public DateTime? CreatedOn { get; set; }
        
        public string? Text { get; set; }
    }
}