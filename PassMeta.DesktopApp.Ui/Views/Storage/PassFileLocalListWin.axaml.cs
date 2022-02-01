namespace PassMeta.DesktopApp.Ui.Views.Storage
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using Common.Models.Entities;
    using Utils.Extensions;
    using ViewModels.Storage.PassFileLocalListWin;

    public class PassFileLocalListWin : Window
    {
        private new PassFileLocalListWinViewModel? DataContext
        {
            get => (PassFileLocalListWinViewModel?)base.DataContext;
            init => base.DataContext = value;
        }
        
        public PassFileLocalListWin()
        {
            AvaloniaXamlLoader.Load(this);
            this.CorrectMainWindowFocusWhileOpened();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public PassFileLocalListWin(PassFile currentPassFile) : this()
        {
            DataContext = new PassFileLocalListWinViewModel(currentPassFile);
            DataContext.ViewElements.Window = this;
            
            Opened += async (_, _) 
                => await DataContext.LoadAsync();
        }
    }
}