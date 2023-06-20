using PassMeta.DesktopApp.Common.Models.Entities.PassFileMerge;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Previews.Data;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileMergeWin;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Previews;

/// <inheritdoc />
public class PassFileMergeWinModelPreview : PassFileMergeWinModel
{
    public PassFileMergeWinModelPreview() : base(
        new PwdPassFileMerge(
            PassFilePreviewData.GetPassFile(),
            PassFilePreviewData.GetPassFile()))
    {
    }
}