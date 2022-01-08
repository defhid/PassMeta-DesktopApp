namespace PassMeta.DesktopApp.Ui.Views.Main
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Input;
    using Avalonia.Interactivity;
    using Avalonia.Markup.Xaml;
    using Common.Enums;
    using ViewModels.Main.DialogWindow;

    public class DialogWindow : Window
    {
        public DialogButton ResultButton { get; private set; }

        private bool _focused;

        public DialogWindow()
        {
            ResultButton = DialogButton.Close;
            AvaloniaXamlLoader.Load(this);
        }
        
        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            var buttonKind = ((Button)sender!).Tag!;
            ResultButton = (DialogButton)buttonKind;
            Close();
        }

        private void Input_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var dataContext = (DialogWindowViewModel)DataContext!;

            if (dataContext.BtnOk.IsVisible)
            {
                ResultButton = DialogButton.Ok;
                Close();
            }
            else if (dataContext.BtnYes.IsVisible)
            {
                ResultButton = DialogButton.Yes;
                Close();
            }
        }

        private void Button_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (_focused) return;
            var btn = (Button)sender!;
            if ((DialogButton)btn.Tag! == ((DialogWindowViewModel)DataContext!).BtnFocused)
            {
                btn.Focus();
            }
        }

        private void Input_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (_focused) return;
            var dataContext = (DialogWindowViewModel)DataContext!;

            if (dataContext.WindowTextInputBox.Visible || dataContext.WindowNumericInputBox.Visible)
            {
                ((Control)sender!).Focus();
                _focused = true;
            }
        }
    }
}