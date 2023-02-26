using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Extensions;

namespace PassMeta.DesktopApp.Ui.Views.Storage;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Base;
using ViewModels.Storage.PassFileWin;

public class PassFileWin : WinView<PassFileWinViewModel>
{
    public PassFile? PassFile { get; private set; }

    private readonly bool _changeNameAdvice;

    public PassFileWin()
    {
        AvaloniaXamlLoader.Load(this);
        this.CorrectMainWindowFocusWhileOpened();
    }

    public PassFileWin(PwdPassFile passFile) : this()
    {
        PassFile = passFile;
        ViewModel = new PassFileWinViewModel(passFile)
        {
            ViewElements = { Window = this }
        };

        _changeNameAdvice = PassFile!.Name.Trim() == Common.Resources.PASSCONTEXT__DEFAULT_NEW_PASSFILE_NAME;
    }

    private void NameTextBox__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var textBox = (sender as TextBox)!;

        textBox.CaretIndex = textBox.Text.Length;

        if (_changeNameAdvice)
        {
            textBox.SelectionStart = 0;
            textBox.SelectionEnd = textBox.Text.Length;
            textBox.Focus();
        }
    }
        
    private void OkBtn__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (!_changeNameAdvice) (sender as Button)!.Focus();
    }
}