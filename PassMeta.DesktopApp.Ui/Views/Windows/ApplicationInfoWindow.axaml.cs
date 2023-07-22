using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows;

namespace PassMeta.DesktopApp.Ui.Views.Windows;

public partial class ApplicationInfoWindow : ReactiveWindow<AppInfoWinModel>
{
    public ApplicationInfoWindow()
    {
        InitializeComponent();
        this.CorrectMainWindowFocusWhileOpened();
    }
}