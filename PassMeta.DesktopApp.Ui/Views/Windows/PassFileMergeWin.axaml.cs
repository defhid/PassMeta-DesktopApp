using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFileMerge;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileMergeWin;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views.Windows;

public partial class PassFileMergeWin : ReactiveWindow<PassFileMergeWinModel>
{
    private readonly IHostWindowProvider _hostWindowProvider = Locator.Current.Resolve<IHostWindowProvider>();
    
    public PassFileMergeWin()
    {
        InitializeComponent();
        this.CorrectMainWindowFocusWhileOpened(_hostWindowProvider);
    }

    public PassFileMergeWin(PwdPassFileMerge passFileMerge) : this()
    {
        DataContext = new PassFileMergeWinModel(passFileMerge)
        {
            ViewElements = { Window = this }
        };
    }
}