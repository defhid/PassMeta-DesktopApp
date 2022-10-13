namespace PassMeta.DesktopApp.Ui.ViewModels.Journal.Models
{
    using Common.Models.Entities;
    using Common.Utils.Extensions;

    public class JournalRecordInfo
    {
        private readonly JournalRecord _record;
        
        public string CreatedOn => _record.TimeStamp.ToLocalTime().ToShortDateTimeString();
        
        public string RecordKind => _record.Kind;
        
        public string UserIp => _record.UserIp;

        public string UserLogin => _record.UserLogin ?? "?";

        public string More => _record.More;  // TODO: add optional passfile info

        public JournalRecordInfo(JournalRecord journalRecord)
        {
            _record = journalRecord;
        }
    }
}