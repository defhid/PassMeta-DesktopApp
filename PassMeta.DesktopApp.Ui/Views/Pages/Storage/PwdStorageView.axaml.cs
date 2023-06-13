using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage;

namespace PassMeta.DesktopApp.Ui.Views.Pages.Storage;

public class PwdStorageView : ReactiveUserControl<PwdStoragePageModel>
{
    public PwdStorageView()
    {
        AvaloniaXamlLoader.Load(this);
    }

    #region Context menus

    private void EditCurrentSection_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel!.SelectedData.ItemsEdit();
    }

    private async void DeleteCurrentSection_OnClick(object? sender, RoutedEventArgs e)
    {
        await ViewModel!.SelectedData.SectionDeleteAsync();
    }

    #endregion

    private void Search_OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        ViewModel!.ViewElements.SearchBox = (TextBox)sender!;
    }

    private void SectionListBox_OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        ViewModel!.ViewElements.SectionListBox = (ListBox)sender!;
    }
}