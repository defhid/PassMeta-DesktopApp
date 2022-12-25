namespace PassMeta.DesktopApp.Ui.ViewModels.Journal.Models
{
    using System.Linq;
    using Common.Models.Dto.Response;
    using Common.Utils.Extensions;

    public class JournalRecordInfo
    {
        private readonly JournalRecord _record;
        
        public string WrittenOn => _record.WrittenOn.ToLocalTime().ToShortDateTimeString();
        
        public string RecordKind => _record.Kind;
        
        public string User => _record.UserLogin ?? (_record.UserId.HasValue ? $"#{_record.UserId}" : "?");
        
        public string UserIp => _record.UserIp;

        public string More => string.Join("; ", new []
        {
            _record.More,
            _record.AffectedPassFileId.HasValue ? $"Passfile #{_record.AffectedPassFileId} '{_record.AffectedPassFileName}'" : null
        }.Where(x => !string.IsNullOrEmpty(x)));

        public JournalRecordInfo(JournalRecord journalRecord)
        {
            _record = journalRecord;
        }
    }
}