using PassMeta.DesktopApp.Common.Models.Entities.PassFileMerge;
using PassMeta.DesktopApp.Ui.Extensions;

namespace PassMeta.DesktopApp.Ui.Views.Storage;

using Avalonia.Markup.Xaml;
using Base;
using ViewModels.Storage.PassFileMergeWin;

public class PassFileMergeWin : WinView<PassFileMergeWinViewModel>
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