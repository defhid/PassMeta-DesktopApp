using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.DialogWin;

namespace PassMeta.DesktopApp.Ui.Views.Windows;

public partial class DialogWindow : ReactiveWindow<DialogWinModel>
{
    public DialogWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var buttonDataContext = (sender as Button)?.DataContext as ResultButton;
        ViewModel!.Result = buttonDataContext?.ButtonKind;
        Close();
    }

    private void Input_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ViewModel!.Result = ViewModel.ButtonPrimary?.ButtonKind;
            Close();
        }
    }

    private void Control_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control control && (
                ReferenceEquals(control.DataContext, ViewModel?.InputBoxFocused) || 
                ReferenceEquals(control.DataContext, ViewModel?.ButtonPrimary)))
        {
            control.Focus();
        }
    }
}