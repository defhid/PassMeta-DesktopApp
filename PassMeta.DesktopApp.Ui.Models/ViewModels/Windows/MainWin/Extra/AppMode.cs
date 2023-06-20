using System;
using System.Reactive.Linq;
using Avalonia.Media;
using PassMeta.DesktopApp.Common;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin.Extra;

/// <summary>
/// Application mode.
/// </summary>
public class AppMode
{
    public IObservable<string> Text { get; }

    public IObservable<ISolidColorBrush> Foreground { get; }

    public AppMode(IObservable<bool> isOnline)
    {
        Text = isOnline.Select(online => online 
            ? Resources.APP__ONLINE_MODE
            : Resources.APP__OFFLINE_MODE);

        Foreground = isOnline.Select(online => online 
            ? Brushes.Green 
            : Brushes.SlateGray);
    }
}