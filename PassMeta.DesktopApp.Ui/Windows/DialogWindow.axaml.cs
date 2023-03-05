using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.DialogWindowModels;

namespace PassMeta.DesktopApp.Ui.Windows;

public class DialogWindow : ReactiveWindow<DialogWindowViewModel>
{
    public DialogWindow()
    {
#if DEBUG
        this.AttachDevTools();
#endif
        AvaloniaXamlLoader.Load(this);
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