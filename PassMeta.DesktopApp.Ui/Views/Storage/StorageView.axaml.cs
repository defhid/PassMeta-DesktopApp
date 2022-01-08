namespace PassMeta.DesktopApp.Ui.Views.Storage
{
    using Avalonia.Markup.Xaml;
    using Base;
    using ViewModels.Storage.Storage;

    public class StorageView : ViewPage<StorageViewModel>
    {
        public StorageView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}