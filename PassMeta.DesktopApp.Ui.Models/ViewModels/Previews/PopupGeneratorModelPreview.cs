using System.Reactive.Linq;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Previews;

/// <inheritdoc />
public class PopupGeneratorModelPreview : PopupGeneratorModel
{
    /// <summary></summary>
    public PopupGeneratorModelPreview() : base(Observable.Return(true), _ => {})
    {
    }
}