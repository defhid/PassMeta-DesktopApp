using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Ui.ViewModels;

namespace PassMeta.DesktopApp.Ui.Views.Main
{
    public class DialogWindow : Window
    {
        public DialogButton ResultButton { get; private set; }

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

        private void Button_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            var btn = (Button)sender!;
            if (btn.Tag as string == ((DialogWindowViewModel)DataContext!).BtnFocusedTag)
            {
                btn.Focus();
            }
        }
    }
}