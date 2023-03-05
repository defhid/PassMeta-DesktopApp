using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileRestoreWin;

namespace PassMeta.DesktopApp.Ui.Views.Windows;

public class PassFileRestoreWin : ReactiveWindow<PassFileRestoreWinModel>
{
    public PassFileRestoreWin()
    {
        AvaloniaXamlLoader.Load(this);
        this.CorrectMainWindowFocusWhileOpened();
    }

    public PassFileRestoreWin(PwdPassFile currentPassFile) : this()
    {
        ViewModel = new PassFileRestoreWinModel(currentPassFile)
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