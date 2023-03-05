using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileWin;

namespace PassMeta.DesktopApp.Ui.Views.Windows;

public class PassFileWin : ReactiveWindow<PassFileWinViewModel>
{
    public PassFileWin()
    {
        AvaloniaXamlLoader.Load(this);
        this.CorrectMainWindowFocusWhileOpened();
    }

    private void NameTextBox__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        ViewModel!.ViewElements.Window = this;
        
        var textBox = (sender as TextBox)!;

        textBox.CaretIndex = textBox.Text.Length;

        if (ViewModel!.PassFile!.Name.Trim() == Common.Resources.PASSCONTEXT__DEFAULT_NEW_PASSFILE_NAME)
        {
            textBox.SelectionStart = 0;
            textBox.SelectionEnd = textBox.Text.Length;
            textBox.Focus();
        }
    }

    private void OkBtn__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (ViewModel!.PassFile!.Name.Trim() != Common.Resources.PASSCONTEXT__DEFAULT_NEW_PASSFILE_NAME)
        {
            (sender as Button)!.Focus();
        }
    }
}