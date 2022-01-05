namespace PassMeta.DesktopApp.Ui.Views.Storage
{
    using Avalonia.Interactivity;
    using Avalonia.Markup.Xaml;
    using Base;
    using ViewModels.Storage;
    using ViewModels.Storage.Storage;

    public class StorageView : ViewPage<StorageViewModel>
    {
        public StorageView()
        {
            AvaloniaXamlLoader.Load(this);
        }

        #region Crutch (reactive command is blocked in menuitem within style setter)
        
        private async void SectionRenameMenuItem_OnClick(object? sender, RoutedEventArgs e)
            => await DataContext!.SectionRenameAsync();
        
        private void SectionDeleteMenuItem_OnClick(object? sender, RoutedEventArgs e)
            => DataContext!.SectionDelete();
        
        #endregion
    }
}