using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows;

namespace PassMeta.DesktopApp.Ui.Views.Windows;

public class ApplicationInfoWindow : ReactiveWindow<AppInfoWinModel>
{
    public ApplicationInfoWindow()
    {
        AvaloniaXamlLoader.Load(this);
        this.CorrectMainWindowFocusWhileOpened();
    }
}