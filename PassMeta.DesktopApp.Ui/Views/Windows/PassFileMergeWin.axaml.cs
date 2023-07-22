using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Common.Models.Entities.PassFileMerge;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileMergeWin;

namespace PassMeta.DesktopApp.Ui.Views.Windows;

public partial class PassFileMergeWin : ReactiveWindow<PassFileMergeWinModel>
{
    public PassFileMergeWin()
    {
        InitializeComponent();
        this.CorrectMainWindowFocusWhileOpened();
    }

    public PassFileMergeWin(PwdPassFileMerge passFileMerge) : this()
    {
        DataContext = new PassFileMergeWinModel(passFileMerge)
        {
            ViewElements = { Window = this }
        };
    }
}