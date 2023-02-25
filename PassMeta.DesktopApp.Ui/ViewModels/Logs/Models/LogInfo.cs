using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Mapping.Values;

namespace PassMeta.DesktopApp.Ui.ViewModels.Logs.Models
{
    using System;
    using Common.Models.Entities;

    public class LogInfo
    {
        private readonly Log? _log;

        public string CreatedOnShort => _log?.CreatedOn?.ToShortDateTimeString() ?? string.Empty;
        public string? CreatedOnFull => _log?.CreatedOn?.ToString("F").Capitalize();

        public string SectionShort => LogSectionMapping.SectionToShortName.Map(_log?.Section, "?");
        public string? SectionFull => LogSectionMapping.SectionToFullName.Map(_log?.Section, _log?.Section);

        public string TextShort
        {
            get
            {
                var text = _log?.Text ?? string.Empty;
                return text.Length <= 60 
                    ? text 
                    : text[..60] + "...";
            }
        }
        public string? TextFull => _log?.Text;
        
        public LogInfo(Log? log)
        {
            _log = log;
        }
        
        public static double DateWidth { get; private set; }
        public static double SectionWidth { get; private set; }

        public static void RefreshStatics()
        {
            DateWidth = new DateTime(2222, 12, 22).ToShortDateTimeString().Length * 7.55;
            SectionWidth = Log.Sections.MappingShort.GetMappings().Max(map => map.To.TrimEnd('.').Length) * 14;
        }
    }
}