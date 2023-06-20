using PassMeta.DesktopApp.Ui.Models.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Previews;

public class PwdStorageModelPreview : PwdStorageModel
{
    public PwdStorageModelPreview() : base(null!, HostWindowProvider.Empty)
    {
    }
}