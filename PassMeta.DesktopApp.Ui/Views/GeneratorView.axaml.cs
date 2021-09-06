using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Ui.ViewModels;
using PassMeta.DesktopApp.Ui.Views.Base;

namespace PassMeta.DesktopApp.Ui.Views
{
    // ReSharper disable once UnusedType.Global
    public class GeneratorView : ViewPage<GeneratorViewModel>
    {
        public GeneratorView()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void GenerateBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            
        }

        private void CopyBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            
        }
    }
}