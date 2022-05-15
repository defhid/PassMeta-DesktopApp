namespace PassMeta.DesktopApp.Ui.Views
{
    using DesktopApp.Ui.ViewModels;
    using DesktopApp.Ui.Views.Base;
    using Avalonia.Markup.Xaml;
    
    public class GeneratorView : PageView<GeneratorViewModel>
    {
        public GeneratorView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}