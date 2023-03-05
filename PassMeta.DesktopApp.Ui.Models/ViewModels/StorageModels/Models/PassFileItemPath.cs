using System;

namespace PassMeta.DesktopApp.Ui.Models.StorageModels.Models;

public class PassFileItemPath
{
    private long? _passFileId;
    public long? PassFileId
    {
        get => _passFileId;
        set
        {
            _passFileId = value;
            PassFileSectionId = null;
        }
    }

    public Guid? PassFileSectionId { get; set; }

    public PassFileItemPath Copy() => (PassFileItemPath)MemberwiseClone();
}