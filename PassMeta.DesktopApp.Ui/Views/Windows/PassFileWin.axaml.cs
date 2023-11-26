using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileWin;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views.Windows;

// TODO
public partial class PassFileWin : ReactiveWindow<PassFileWinModel<PwdPassFile>>
{
    private readonly IHostWindowProvider _hostWindowProvider = Locator.Current.Resolve<IHostWindowProvider>();

    public PassFileWin()
    {
        InitializeComponent();
        this.CorrectMainWindowFocusWhileOpened(_hostWindowProvider);

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