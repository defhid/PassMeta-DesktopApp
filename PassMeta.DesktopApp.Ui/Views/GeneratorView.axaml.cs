using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.ViewModels;

namespace PassMeta.DesktopApp.Ui.Views
{
    public class GeneratorView : ReactiveUserControl<GeneratorViewModel>
    {
        public GeneratorView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}