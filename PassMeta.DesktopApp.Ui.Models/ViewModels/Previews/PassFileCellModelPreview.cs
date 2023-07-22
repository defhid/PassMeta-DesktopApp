using System.Reactive.Linq;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Previews.Data;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Previews;

/// <inheritdoc />
public class PassFileCellModelPreview : PassFileCellModel
{
    public PassFileCellModelPreview() 
        : base(
            PassFilePreviewData.GetPassFile<PwdPassFile>(), 
            Observable.Return(false), 
            null!)
    {
    }
}