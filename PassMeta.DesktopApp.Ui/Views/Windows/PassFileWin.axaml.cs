using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileWin;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Views.Windows;

public class PassFileWin : ReactiveWindow<PassFileWinModel>
{
    public PassFileWin()
    {
        AvaloniaXamlLoader.Load(this);
        this.CorrectMainWindowFocusWhileOpened();

        this.WhenActivated(_ => ViewModel!.Finish += Close);
    }

    private void NameTextBox__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var textBox = (sender as TextBox)!;

        textBox.CaretIndex = textBox.Text.Length;

        if (ViewModel!.PassFile.IsLocalCreated())
        {
            textBox.SelectionStart = 0;
            textBox.SelectionEnd = textBox.Text.Length;
            textBox.Focus();
        }
    }

    private void OkBtn__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (ViewModel!.PassFile.IsLocalCreated())
        {
            (sender as Button)!.Focus();
        }
    }
}