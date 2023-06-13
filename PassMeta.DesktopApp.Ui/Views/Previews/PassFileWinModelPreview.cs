using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileWin;
using PassMeta.DesktopApp.Ui.Views.Previews.Data;

namespace PassMeta.DesktopApp.Ui.Views.Previews;

/// <inheritdoc />
public class PassFileWinModelPreview : PassFileWinModel<PwdPassFile>
{
    /// <summary></summary>
    public PassFileWinModelPreview()
        : base(PassFilePreviewData.GetPassFile<PwdPassFile>(), HostWindowProvider.Empty)
    {
    }
}