namespace PassMeta.DesktopApp.Ui.ViewModels.Journal.Models
{
    public class JournalRecordKindInfo
    {
        public int Id { get; }
        
        public string Name { get; }
        
        public JournalRecordKindInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString() => Name;
    }
}