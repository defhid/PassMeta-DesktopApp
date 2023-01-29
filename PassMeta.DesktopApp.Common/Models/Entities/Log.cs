using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;
using PassMeta.DesktopApp.Common.Utils.ValueMapping;

namespace PassMeta.DesktopApp.Common.Models.Entities;

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
            
        /// <summary>
        /// Mapping for <see cref="Section"/> values.
        /// </summary>
        public static readonly IValuesMapper<string, string> Mapping = new ValuesMapper<string, string>(
            new MapToResource<string>[]
            {
                new(Info, () => Resources.LOG_SECTION__INFO),
                new(Warning, () => Resources.LOG_SECTION__WARN),
                new(Error, () => Resources.LOG_SECTION__ERROR),
                new(Unknown, () => Resources.LOG_SECTION__UNKNOWN),
            });
            
        /// <summary>
        /// Short mapping for <see cref="Section"/> values.
        /// </summary>
        public static readonly IValuesMapper<string, string> MappingShort = new ValuesMapper<string, string>(
            new MapToResource<string>[]
            {
                new(Info, () => Resources.LOG_SECTION__INFO_SHORT),
                new(Warning, () => Resources.LOG_SECTION__WARN_SHORT),
                new(Error, () => Resources.LOG_SECTION__ERROR_SHORT),
                new(Unknown, () => Resources.LOG_SECTION__UNKNOWN_SHORT),
            });
    }
}