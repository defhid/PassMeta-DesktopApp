using Avalonia.Interactivity;
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

        #region Crutch (reactive command is blocked in menuitem within style setter)

        private async void PassFileRenameMenuItem_OnClick(object? sender, RoutedEventArgs e)
            => await DataContext!.PassFileRenameAsync();

        private async void PassFileArchiveMenuItem_OnClick(object? sender, RoutedEventArgs e)
            => await DataContext!.PassFileArchiveAsync();

        private async void PassFileDeleteMenuItem_OnClick(object? sender, RoutedEventArgs e)
            => await DataContext!.PassFileDeleteAsync();

        private async void SectionRenameMenuItem_OnClick(object? sender, RoutedEventArgs e)
            => await DataContext!.SectionRenameAsync();
        
        private void SectionDeleteMenuItem_OnClick(object? sender, RoutedEventArgs e)
            => DataContext!.SectionDelete();
        
        #endregion
    }
}