namespace PassMeta.DesktopApp.Ui.ViewModels.Journal.Models
{
    using Common.Models.Entities;
    using Common.Utils.Extensions;

    public class JournalRecordInfo
    {
        private readonly JournalRecord _record;
        
        public string CreatedOn => _record.TimeStamp.ToShortDateTimeString();
        
        public string RecordKind => _record.Kind;

        public string UserLogin => _record.UserLogin ?? "?";

        public string More => _record.More ?? string.Empty;

        public JournalRecordInfo(JournalRecord journalRecord)
        {
            _record = journalRecord;
        }
    }
}