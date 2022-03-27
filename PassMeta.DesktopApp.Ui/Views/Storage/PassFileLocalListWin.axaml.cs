namespace PassMeta.DesktopApp.Ui.Views.Storage
{
    using Avalonia.Markup.Xaml;
    using Common.Models.Entities;
    using Utils.Extensions;
    using ViewModels.Base;
    using ViewModels.Storage.PassFileLocalListWin;

    public class PassFileLocalListWin : WinView<PassFileLocalListWinViewModel>
    {
        public PassFileLocalListWin()
        {
            AvaloniaXamlLoader.Load(this);
            this.CorrectMainWindowFocusWhileOpened();
        }

        public PassFileLocalListWin(PassFile currentPassFile) : this()
        {
            DataContext = new PassFileLocalListWinViewModel(currentPassFile)
            {
                ViewElements = { Window = this }
            };

            Opened += (_, _) => DataContext.Load();
        }
    }
}