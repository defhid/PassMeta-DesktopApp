using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileRestoreWin;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views.Windows;

public partial class PassFileRestoreWin : ReactiveWindow<PassFileRestoreWinModel>
{
    private readonly IHostWindowProvider _hostWindowProvider = Locator.Current.Resolve<IHostWindowProvider>();

    public PassFileRestoreWin()
    {
        InitializeComponent();
        this.CorrectMainWindowFocusWhileOpened(_hostWindowProvider);
    }

    public PassFileRestoreWin(PwdPassFile currentPassFile) : this()
    {
        ViewModel = new PassFileRestoreWinModel(currentPassFile)
        {
            ViewElements = { Window = this }
        };
    }
}