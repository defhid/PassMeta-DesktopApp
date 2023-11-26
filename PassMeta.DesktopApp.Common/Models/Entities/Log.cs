using System;

namespace PassMeta.DesktopApp.Common.Models.Entities;

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

    /// <summary>
    /// Defined values for <see cref="Log.Section"/>.
    /// </summary>
    public static class Sections
    {
        /// <summary>
        /// Informational section.
        /// </summary>
        public const string Debug = "DG";

        /// <summary>
        /// Informational section.
        /// </summary>
        public const string Info = "IN";
            
        /// <summary>
        /// Warning section.
        /// </summary>
        public const string Warning = "WA";
            
        /// <summary>
        /// Error section.
        /// </summary>
        public const string Error = "ER";
            
        /// <summary>
        /// Unknown section.
        /// </summary>
        public const string Unknown = "UN";
    }
}