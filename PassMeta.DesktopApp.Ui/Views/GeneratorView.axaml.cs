using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models;

namespace PassMeta.DesktopApp.Ui.Views;

public class GeneratorView : ReactiveUserControl<GeneratorModel>
{
    public GeneratorView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}