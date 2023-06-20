using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Previews.Data;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileWin;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Previews;

/// <inheritdoc />
public class PassFileWinModelPreview : PassFileWinModel<PwdPassFile>
{
    public PassFileWinModelPreview()
        : base(PassFilePreviewData.GetPassFile<PwdPassFile>(), HostWindowProvider.Empty)
    {
    }
}