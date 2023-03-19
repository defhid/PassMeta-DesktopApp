using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

namespace PassMeta.DesktopApp.Ui.Views.Pages.Storage.Components;

public class PwdItemReadCard : ReactiveUserControl<PwdItemReadCardModel>
{
    public PwdItemReadCard()
    {
        AvaloniaXamlLoader.Load(this);
    }
}