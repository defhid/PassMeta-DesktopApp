using System.Reactive.Linq;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;
using PassMeta.DesktopApp.Ui.Views.Previews.Data;

namespace PassMeta.DesktopApp.Ui.Views.Previews;

/// <inheritdoc />
public class PassFileCellModelPreview : PassFileCellModel<PwdPassFile>
{
    /// <summary></summary>
    public PassFileCellModelPreview() 
        : base(
            PassFilePreviewData.GetPassFile<PwdPassFile>(), 
            Observable.Return(false), 
            HostWindowProvider.Empty)
    {
    }
}