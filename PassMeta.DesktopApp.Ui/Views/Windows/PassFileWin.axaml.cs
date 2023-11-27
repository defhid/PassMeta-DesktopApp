using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileWin;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views.Windows;

public partial class PassFileWin : ReactiveWindow<PassFileWinModel>
{
    private readonly IHostWindowProvider _hostWindowProvider = Locator.Current.Resolve<IHostWindowProvider>();

    public PassFileWin()
    {
        InitializeComponent();
        this.CorrectMainWindowFocusWhileOpened(_hostWindowProvider);

        this.WhenActivated(disposables =>
            ViewModel!.Quit
                .RegisterHandler(context => context.SetOutput(Close))
                .DisposeWith(disposables));
    }

    private void NameTextBox__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var textBox = (sender as TextBox)!;

        textBox.CaretIndex = textBox.Text?.Length ?? 0;

        if (ViewModel!.PassFile.IsLocalCreated())
        {
            textBox.SelectionStart = 0;
            textBox.SelectionEnd = textBox.Text?.Length ?? 0;
            textBox.Focus();
        }
    }

    private void OkBtn__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (!ViewModel!.PassFile.IsLocalCreated())
        {
            (sender as Button)!.Focus();
        }
    }
}