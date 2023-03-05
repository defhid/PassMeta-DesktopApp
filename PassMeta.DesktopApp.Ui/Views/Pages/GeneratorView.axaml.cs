using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages;

namespace PassMeta.DesktopApp.Ui.Views.Pages;

public class GeneratorView : ReactiveUserControl<GeneratorPageModel>
{
    public GeneratorView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}