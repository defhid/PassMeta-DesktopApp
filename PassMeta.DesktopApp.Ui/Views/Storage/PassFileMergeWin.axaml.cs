namespace PassMeta.DesktopApp.Ui.Views.Storage
{
    using Avalonia.Markup.Xaml;
    using Base;
    using Common.Models.Dto;
    using Utils.Extensions;
    using ViewModels.Storage.PassFileMergeWin;

    public class PassFileMergeWin : WinView<PassFileMergeWinViewModel>
    {
        public PassFileMergeWin()
        {
            AvaloniaXamlLoader.Load(this);
            this.CorrectMainWindowFocusWhileOpened();
        }

        public PassFileMergeWin(PwdMerge merge) : this()
        {
            DataContext = new PassFileMergeWinViewModel(merge)
            {
                ViewElements = { Window = this }
            };
        }
    }
}