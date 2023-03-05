using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Common.Models.Entities.PassFileMerge;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.PassFileMergeWin;

namespace PassMeta.DesktopApp.Ui.Windows;

public class PassFileMergeWin : ReactiveWindow<PassFileMergeWinViewModel>
{
    public PassFileMergeWin()
    {
        AvaloniaXamlLoader.Load(this);
        this.CorrectMainWindowFocusWhileOpened();
    }

    public PassFileMergeWin(PwdPassFileMerge passFileMerge) : this()
    {
        DataContext = new PassFileMergeWinViewModel(passFileMerge)
        {
            ViewElements = { Window = this }
        };
    }
}