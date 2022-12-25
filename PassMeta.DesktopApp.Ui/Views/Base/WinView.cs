namespace PassMeta.DesktopApp.Ui.Views.Base
{
    using Avalonia;
    using Avalonia.Controls;

    public abstract class WinView<TViewModel> : Window
    {
        public new TViewModel? DataContext
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