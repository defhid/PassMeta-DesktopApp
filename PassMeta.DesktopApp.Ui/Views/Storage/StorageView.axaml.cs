namespace PassMeta.DesktopApp.Ui.Views.Storage
{
    using Avalonia.Controls;
    using Avalonia.Interactivity;
    using Avalonia.LogicalTree;
    using Avalonia.Markup.Xaml;
    using Base;
    using ViewModels.Storage.Storage;

    public class StorageView : ViewPage<StorageViewModel>
    {
        public StorageView()
        {
            AvaloniaXamlLoader.Load(this);
        }

        #region Context menus

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

        #endregion

        private void SectionNameEdit_OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
        {
            DataContext!.ViewElements.SectionNameEditBox = (TextBox)sender!;
        }

        private void Search_OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
        {
            DataContext!.ViewElements.SearchBox = (TextBox)sender!;
        }

        private void ItemScrollViewer_OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
        {
            DataContext!.ViewElements.ItemScrollViewer = (ScrollViewer)sender!;
        }

        private void SectionListBox_OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
        {
            DataContext!.ViewElements.SectionListBox = (ListBox)sender!;
        }
    }
}