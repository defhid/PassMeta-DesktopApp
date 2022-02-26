namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.PassFileLocalListWin.Models
{
    public class DataFile
    {
        public string? Name { get; init; }
        
        public string? Description { get; init; }
        
        public string FilePath { get; }

        public DataFile(string filePath)
        {
            FilePath = filePath;
        }
    }
}