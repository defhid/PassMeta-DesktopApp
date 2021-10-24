using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Ui.ViewModels.Storage;
using PassMeta.DesktopApp.Ui.Views.Base;

namespace PassMeta.DesktopApp.Ui.Views
{
    public class StorageView : ViewPage<StorageViewModel>
    {
        public StorageView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}