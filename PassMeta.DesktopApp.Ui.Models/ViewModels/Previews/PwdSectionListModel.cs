using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Previews.Data;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Previews;

/// <inheritdoc />
public class PwdSectionListModelPreview : PwdSectionListModel
{
    public PwdSectionListModelPreview()
    {
        RefreshList(new[]
        {
            PassFilePreviewData.PwdSection,
            PassFilePreviewData.PwdSection,
            PassFilePreviewData.PwdSection
        });
    }
}