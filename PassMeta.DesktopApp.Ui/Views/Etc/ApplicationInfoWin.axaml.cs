using PassMeta.DesktopApp.Ui.Extensions;

namespace PassMeta.DesktopApp.Ui.Views.Etc;

using Avalonia.Markup.Xaml;
using Base;
using ViewModels.Etc;

public class ApplicationInfoWin : WinView<ApplicationInfoViewModel>
{
    public ApplicationInfoWin()
    {
        AvaloniaXamlLoader.Load(this);
        this.CorrectMainWindowFocusWhileOpened();
            
        DataContext = new ApplicationInfoViewModel();
    }
}