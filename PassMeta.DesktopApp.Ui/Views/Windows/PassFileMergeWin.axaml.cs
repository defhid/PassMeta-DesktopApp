using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Common.Models.Entities.PassFileMerge;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileMergeWin;

namespace PassMeta.DesktopApp.Ui.Views.Windows;

public class PassFileMergeWin : ReactiveWindow<PassFileMergeWinModel>
{
    public PassFileMergeWin()
    {
        AvaloniaXamlLoader.Load(this);
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