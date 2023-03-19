using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Mapping.Values;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.LogsPage.Extra;

/// <summary>
/// Log information.
/// </summary>
public class LogInfo
{
    private readonly Log? _log;    

    /// <summary></summary>
    public LogInfo(Log? log)
    {
        _log = log;
    }

    /// <summary></summary>
    public string CreatedOnShort => _log?.CreatedOn?.ToShortDateTimeString() ?? string.Empty;
    
    /// <summary></summary>
    public string? CreatedOnFull => _log?.CreatedOn?.ToString("F").Capitalize();

    /// <summary></summary>
    public string SectionShort => LogSectionMapping.SectionToShortName.Map(_log?.Section, "?");

    /// <summary></summary>
    public string? SectionFull => LogSectionMapping.SectionToFullName.Map(_log?.Section, _log?.Section);

    /// <summary></summary>
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

    /// <summary></summary>
    public string? TextFull => _log?.Text;
}