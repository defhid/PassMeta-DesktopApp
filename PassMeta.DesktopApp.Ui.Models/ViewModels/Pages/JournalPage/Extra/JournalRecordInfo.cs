using System.Linq;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Dto.Response;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.JournalPage.Extra;

public class JournalRecordInfo
{
    private readonly JournalRecordDto _recordDto;
        
    public string WrittenOn => _recordDto.WrittenOn.ToLocalTime().ToShortDateTimeString();
        
    public string RecordKind => _recordDto.Kind;
        
    public string User => _recordDto.UserLogin ?? (_recordDto.UserId.HasValue ? $"#{_recordDto.UserId}" : "?");
        
    public string UserIp => _recordDto.UserIp;

    public string More => string.Join("; ", new []
    {
        _recordDto.More,
        _recordDto.AffectedPassFileId.HasValue ? $"Passfile #{_recordDto.AffectedPassFileId} '{_recordDto.AffectedPassFileName}'" : null
    }.Where(x => !string.IsNullOrEmpty(x)));

    public JournalRecordInfo(JournalRecordDto journalRecordDto)
    {
        _recordDto = journalRecordDto;
    }
}