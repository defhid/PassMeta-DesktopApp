using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PassMeta.DesktopApp.Ui.Views.Windows.MainWin.Extra;

public class MainPreloader : UserControl
{
    public MainPreloader()
    {
        AvaloniaXamlLoader.Load(this);
    }
}