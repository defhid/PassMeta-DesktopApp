namespace PassMeta.DesktopApp.Common.Models.Entities
{
    using System;

    /// <summary>
    /// Application log object.
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Log section.
        /// </summary>
        public string? Section { get; set; }
        
        /// <summary>
        /// Log creation date and time.
        /// </summary>
        public DateTime? CreatedOn { get; set; }
        
        /// <summary>
        /// Log content.
        /// </summary>
        public string? Text { get; set; }
    }
}