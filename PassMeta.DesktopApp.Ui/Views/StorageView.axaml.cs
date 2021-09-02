using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.ViewModels;

namespace PassMeta.DesktopApp.Ui.Views
{
    public class StorageView : ReactiveUserControl<StorageViewModel>
    {
        public StorageView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}