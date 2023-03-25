using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

namespace PassMeta.DesktopApp.Ui.Views.Pages.Storage.Components;

public class PwdItemEditView : ReactiveUserControl<PwdItemEditModel>
{
    public PwdItemEditView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}