namespace PassMeta.DesktopApp.Ui.ViewModels.Journal.Models
{
    using Common.Interfaces.Mapping;
    using Common.Models.Entities;
    using Common.Utils.Extensions;

    public class JournalRecordInfo
    {
        private readonly JournalRecord _record;
        private readonly IMapper<int, string> _kindMapper;
        
        public string CreatedOn => _record.TimeStamp.ToShortDateTimeString();
        
        public string RecordKind => _kindMapper.Map(_record.KindId, _record.KindId.ToString());
        
        public string UserLogin => _record.UserLogin ?? "?";

        public string More => _record.More ?? string.Empty;

        public JournalRecordInfo(JournalRecord journalRecord, IMapper<int, string> kindMapper)
        {
            _record = journalRecord;
            _kindMapper = kindMapper;
        }
    }
}