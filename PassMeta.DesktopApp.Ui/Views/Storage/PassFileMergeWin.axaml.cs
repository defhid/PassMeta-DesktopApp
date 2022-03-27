namespace PassMeta.DesktopApp.Ui.Views.Storage
{
    using Avalonia.Markup.Xaml;
    using Common.Models.Dto;
    using Utils.Extensions;
    using ViewModels.Base;
    using ViewModels.Storage.PassFileMergeWin;

    public class PassFileMergeWin : WinView<PassFileMergeWinViewModel>
    {
        public PassFileMergeWin()
        {
            AvaloniaXamlLoader.Load(this);
            this.CorrectMainWindowFocusWhileOpened();
        }

        public PassFileMergeWin(PassFileMerge merge) : this()
        {
            DataContext = new PassFileMergeWinViewModel(merge)
            {
                ViewElements = { Window = this }
            };
        }
    }
}