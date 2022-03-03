namespace PassMeta.DesktopApp.Ui.ViewModels.Journal.Models
{
    public class JournalRecordKind
    {
        public int Id { get; }
        
        public string Name { get; }
        
        public JournalRecordKind(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}