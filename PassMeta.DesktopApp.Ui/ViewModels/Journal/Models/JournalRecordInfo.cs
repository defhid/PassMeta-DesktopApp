using PassMeta.DesktopApp.Common.Extensions;

namespace PassMeta.DesktopApp.Ui.ViewModels.Journal.Models
{
    using System.Linq;
    using Common.Models.Dto.Response;

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
}