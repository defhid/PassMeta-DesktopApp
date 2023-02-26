using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Extensions;

namespace PassMeta.DesktopApp.Ui.Views.Storage;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Ui.Views.Base;
using ViewModels.Storage.PassFileRestoreWin;

public class PassFileRestoreWin : WinView<PassFileRestoreWinViewModel>
{
    public PassFileRestoreWin()
    {
        AvaloniaXamlLoader.Load(this);
        this.CorrectMainWindowFocusWhileOpened();
    }

    public PassFileRestoreWin(PwdPassFile currentPassFile) : this()
    {
        ViewModel = new PassFileRestoreWinViewModel(currentPassFile)
        {
            ViewElements = { Window = this }
        };

        Opened += async (_, _) => await ViewModel!.LoadAsync();
    }

    private void DataGrid_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        ViewModel!.ViewElements.DataGrid = (DataGrid)sender!;
    }
}