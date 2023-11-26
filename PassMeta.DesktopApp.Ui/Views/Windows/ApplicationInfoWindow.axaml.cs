using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views.Windows;

public partial class ApplicationInfoWindow : ReactiveWindow<AppInfoWinModel>
{
    private readonly IHostWindowProvider _hostWindowProvider = Locator.Current.Resolve<IHostWindowProvider>();

    public ApplicationInfoWindow()
    {
        InitializeComponent();
        this.CorrectMainWindowFocusWhileOpened(_hostWindowProvider);
    }
}