using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Ui.Views.Storage
{
    using Avalonia.Markup.Xaml;
    using Base;
    using Utils.Extensions;
    using ViewModels.Storage.PassFileMergeWin;

    public class PassFileMergeWin : WinView<PassFileMergeWinViewModel>
    {
        public PassFileMergeWin()
        {
            AvaloniaXamlLoader.Load(this);
            this.CorrectMainWindowFocusWhileOpened();
        }

        public PassFileMergeWin(PwdSectionsMerge sectionsMerge) : this()
        {
            DataContext = new PassFileMergeWinViewModel(sectionsMerge)
            {
                ViewElements = { Window = this }
            };
        }
    }
}