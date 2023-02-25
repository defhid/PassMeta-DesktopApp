namespace PassMeta.DesktopApp.Ui.Constants;

using System;
using Avalonia.Input;

public static class EventHandlers
{
    public static readonly EventHandler<PointerPressedEventArgs> FocusElementOnPointerPressed =
        (sender, _) => (sender as InputElement)?.Focus();
}