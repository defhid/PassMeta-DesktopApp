namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage.Models;

public class PassFileItemPath
{
    private int? _passFileId;
    public int? PassFileId
    {
        get => _passFileId;
        set
        {
            _passFileId = value;
            PassFileSectionId = null;
        }
    }

    public string? PassFileSectionId { get; set; }

    public PassFileItemPath Copy() => (PassFileItemPath)MemberwiseClone();
}