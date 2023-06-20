using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage;

namespace PassMeta.DesktopApp.Ui.Views.Pages.Storage;

public class PwdStorageView : ReactiveUserControl<PwdStorageModel>
{
    public PwdStorageView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}