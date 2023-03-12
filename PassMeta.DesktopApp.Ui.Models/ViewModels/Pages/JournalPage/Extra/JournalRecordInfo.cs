using System.Linq;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Dto.Response;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.JournalPage.Extra;

/// <summary>
/// Journal record information.
/// </summary>
public class JournalRecordInfo
{
    private readonly JournalRecordDto _recordDto;

    /// <summary></summary>
    public string WrittenOn => _recordDto.WrittenOn.ToLocalTime().ToShortDateTimeString();

    /// <summary></summary>
    public string RecordKind => _recordDto.Kind;

    /// <summary></summary>
    public string User => _recordDto.UserLogin ?? (_recordDto.UserId.HasValue ? $"#{_recordDto.UserId}" : "?");

    /// <summary></summary>
    public string UserIp => _recordDto.UserIp;

    /// <summary></summary>
    public string More => string.Join("; ", new[]
    {
        _recordDto.More,
        _recordDto.AffectedPassFileId.HasValue
            ? $"Passfile #{_recordDto.AffectedPassFileId} '{_recordDto.AffectedPassFileName}'"
            : null
    }.Where(x => !string.IsNullOrEmpty(x)));

    /// <summary></summary>
    public JournalRecordInfo(JournalRecordDto journalRecordDto)
    {
        _recordDto = journalRecordDto;
    }
}