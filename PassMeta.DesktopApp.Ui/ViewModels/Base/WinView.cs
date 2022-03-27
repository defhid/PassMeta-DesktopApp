namespace PassMeta.DesktopApp.Ui.ViewModels.Base
{
    using Avalonia;
    using Avalonia.Controls;

    public abstract class WinView<TViewModel> : Window
    {
        protected new TViewModel? DataContext
        {
            get => (TViewModel?)base.DataContext;
            init => base.DataContext = value;
        }

        protected WinView()
        {
#if DEBUG
            this.AttachDevTools();
#endif
        }
    }
}