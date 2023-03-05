using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.Etc;

namespace PassMeta.DesktopApp.Ui.Views.Etc;

public class ApplicationInfoWin : ReactiveWindow<ApplicationInfoViewModel>
{
    public ApplicationInfoWin()
    {
        AvaloniaXamlLoader.Load(this);
        this.CorrectMainWindowFocusWhileOpened();
    }
}