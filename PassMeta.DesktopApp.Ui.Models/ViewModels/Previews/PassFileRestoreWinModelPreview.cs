using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Previews.Data;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileRestoreWin;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Previews;

/// <inheritdoc />
public class PassFileRestoreWinModelPreview : PassFileRestoreWinModel
{
    /// <summary></summary>
    public PassFileRestoreWinModelPreview() : base(PassFilePreviewData.GetPassFile<PwdPassFile>())
    {
    }
}