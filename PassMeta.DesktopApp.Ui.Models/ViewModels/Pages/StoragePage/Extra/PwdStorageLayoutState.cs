namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Extra;

public class PwdStorageLayoutState
{
    private PwdStorageLayoutState()
    {
    }

    public double PassFilesPaneWidth { get; private init; }
    
    public static readonly PwdStorageLayoutState Init = new() { PassFilesPaneWidth = 250d };

    public static readonly PwdStorageLayoutState AfterPassFileSelection = new() { PassFilesPaneWidth = 250d };

    public static readonly PwdStorageLayoutState AfterSectionSelection = new() { PassFilesPaneWidth = 200d };
}