using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileWin;
using PassMeta.DesktopApp.Ui.Views.Windows;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Views.Pages.Storage;

public class StorageView : ReactiveUserControl<StoragePageModel>
{
    public StorageView()
    {
        AvaloniaXamlLoader.Load(this);

        this.WhenActivated(d => d(ViewModel!.ShowPassFile.RegisterHandler(ShowPassFileAsync)));
    }
    
    private async Task ShowPassFileAsync(InteractionContext<PassFileWinViewModel, Unit> interaction)
    {
        var win = new PassFileWin { ViewModel = interaction.Input };

        await win.ShowDialog(App.App.MainWindow);

        interaction.SetOutput(Unit.Default);
    }

    #region Context menus

    private async void OpenCurrentPassFile_OnClick(object? sender, RoutedEventArgs e)
    {
        await ViewModel!.PassFileOpenAsync();
    }

    private void EditCurrentSection_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel!.SelectedData.ItemsEdit();
    }

    private async void DeleteCurrentSection_OnClick(object? sender, RoutedEventArgs e)
    {
        await ViewModel!.SelectedData.SectionDeleteAsync();
    }

    #endregion

    private void SectionNameEdit_OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        ViewModel!.ViewElements.SectionNameEditBox = (TextBox)sender!;
    }

    private void Search_OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        ViewModel!.ViewElements.SearchBox = (TextBox)sender!;
    }

    private void ItemScrollViewer_OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        ViewModel!.ViewElements.ItemScrollViewer = (ScrollViewer)sender!;
    }

    private void SectionListBox_OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        ViewModel!.ViewElements.SectionListBox = (ListBox)sender!;
    }
}