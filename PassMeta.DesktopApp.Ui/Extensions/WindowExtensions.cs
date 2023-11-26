using Avalonia.Controls;
using Avalonia.Input;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;

namespace PassMeta.DesktopApp.Ui.Extensions;

/// <summary>
/// Extension methods for <see cref="Window"/>.
/// </summary>
public static class WindowExtensions
{
    /// <summary>
    /// Crutch: dialog windows focus on  after closing,
    /// but if current window is modal (other dialog), focus must be on current.
    /// </summary>
    public static void CorrectMainWindowFocusWhileOpened(this Window win, IHostWindowProvider hostWindowProvider)
    {
        var hostWin = hostWindowProvider.Window;

        win.Opened += (_, _) =>
            hostWin.GotFocus += win._FocusInsteadMainWindow;
        win.Closing += (_, _) =>
            hostWin.GotFocus -= win._FocusInsteadMainWindow;
    }
        
    private static void _FocusInsteadMainWindow(this Window win, object? sender, GotFocusEventArgs e) => win.Activate();
}