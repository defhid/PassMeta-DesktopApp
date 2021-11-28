using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Ui.ViewModels.Main;

namespace PassMeta.DesktopApp.Ui.Views.Main
{
    using Models.Enums;

    public class DialogWindow : Window
    {
        public DialogButton ResultButton { get; private set; }

        private bool _focused;

        public DialogWindow()
        {
            ResultButton = DialogButton.Close;
            AvaloniaXamlLoader.Load(this);
        }

        private void OkButton_OnClick(object? sender, RoutedEventArgs e)
        {
            ResultButton = DialogButton.Ok;
            Close();
        }
        
        private void YesButton_OnClick(object? sender, RoutedEventArgs e)
        {
            ResultButton = DialogButton.Yes;
            Close();
        }
        
        private void NoButton_OnClick(object? sender, RoutedEventArgs e)
        {
            ResultButton = DialogButton.No;
            Close();
        }
        
        private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
        {
            ResultButton = DialogButton.Cancel;
            Close();
        }
        
        private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
        {
            ResultButton = DialogButton.Close;
            Close();
        }
        
        private void Input_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var dataContext = (DialogWindowViewModel)DataContext!;

            if (dataContext.WindowBtnOk.Visible)
            {
                ResultButton = DialogButton.Ok;
                Close();
            }
            else if (dataContext.WindowBtnYes.Visible)
            {
                ResultButton = DialogButton.Yes;
                Close();
            }
        }

        private void Button_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (_focused) return;
            var btn = (Button)sender!;
            if (btn.Tag as string == ((DialogWindowViewModel)DataContext!).BtnFocusedTag)
            {
                btn.Focus();
            }
        }

        private void Input_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (_focused) return;
            var dataContext = (DialogWindowViewModel)DataContext!;

            if (dataContext.WindowTextBox.Visible || dataContext.WindowNumericBox.Visible)
            {
                ((Control)sender!).Focus();
                _focused = true;
            }
        }
    }
}