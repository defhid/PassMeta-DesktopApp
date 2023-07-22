using System;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Extra;

public readonly struct PwdStoragePath
{
    public readonly long? PassFileId;
    
    public readonly Guid? SectionId;

    public PwdStoragePath(long? passFileId = null, Guid? sectionId = null)
    {
        PassFileId = passFileId;
        SectionId = sectionId;
    }

    public static readonly PwdStoragePath Empty = new();
}