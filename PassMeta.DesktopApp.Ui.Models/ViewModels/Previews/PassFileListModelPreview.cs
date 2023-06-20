using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Previews.Data;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Previews;

/// <inheritdoc />
public class PassFileListModelPreview : PassFileListModel<PwdPassFile>
{
    public PassFileListModelPreview() : base(HostWindowProvider.Empty)
    {
        RefreshList(new[]
        {
            PassFilePreviewData.GetPassFile<PwdPassFile>(),
            PassFilePreviewData.GetPassFile<PwdPassFile>(),
            PassFilePreviewData.GetPassFile<PwdPassFile>(),
        });
    }
}