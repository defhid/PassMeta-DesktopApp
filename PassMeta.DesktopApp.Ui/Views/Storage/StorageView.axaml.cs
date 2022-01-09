namespace PassMeta.DesktopApp.Ui.Views.Storage
{
    using Avalonia.Interactivity;
    using Avalonia.Markup.Xaml;
    using Base;
    using ViewModels.Storage.Storage;

    public class StorageView : ViewPage<StorageViewModel>
    {
        public StorageView()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void OpenCurrentPassFile_OnClick(object? sender, RoutedEventArgs e)
        {
            await DataContext!.PassFileOpenAsync();
        }

        private void EditCurrentSection_OnClick(object? sender, RoutedEventArgs e)
        {
            DataContext!.SelectedData.ItemsEdit();
        }

        private async void DeleteCurrentSection_OnClick(object? sender, RoutedEventArgs e)
        {
            await DataContext!.SelectedData.SectionDeleteAsync();
        }
    }
}