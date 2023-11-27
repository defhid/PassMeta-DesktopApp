namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileRestoreWin.Extra;

public class DataFile
{
    public int?  PassFileId { get; init; }
    
    public int?  PassFileVersion { get; init; }
    
    public string? Name { get; init; }
        
    public string? Description { get; init; }
        
    public string FilePath { get; }

    public DataFile(string filePath)
    {
        FilePath = filePath;
    }
}