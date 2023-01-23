namespace PassMeta.DesktopApp.Ui.Views
{
    using ViewModels;
    using Base;
    using Avalonia.Markup.Xaml;
    
    public class GeneratorView : PageView<GeneratorViewModel>
    {
        public GeneratorView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}