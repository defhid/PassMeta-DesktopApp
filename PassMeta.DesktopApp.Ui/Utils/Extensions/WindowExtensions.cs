namespace PassMeta.DesktopApp.Ui.Utils.Extensions
{
    using Avalonia.Controls;
    using Avalonia.Input;
    using Views.Main;

    /// <summary>
    /// Extension methods for <see cref="Window"/>.
    /// </summary>
    public static class WindowExtensions
    {
        /// <summary>
        /// Crutch: dialog windows focus on <see cref="MainWindow"/> after closing,
        /// but if current window is modal (other dialog), focus must be on current.
        /// </summary>
        public static void CorrectMainWindowFocusWhileOpened(this Window win)
        {
            win.Opened += (_, _) =>
                App.App.MainWindow!.GotFocus += win._FocusInsteadMainWindow;
            win.Closing += (_, _) =>
                App.App.MainWindow!.GotFocus -= win._FocusInsteadMainWindow;
        }
        
        private static void _FocusInsteadMainWindow(this Window win, object? sender, GotFocusEventArgs e) => win.Activate();
    }
}