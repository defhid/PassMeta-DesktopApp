using System;
using Avalonia.Input;

namespace PassMeta.DesktopApp.Ui.Models.Common.Constants;

public static class EventHandlers
{
    public static readonly EventHandler<PointerPressedEventArgs> FocusElementOnPointerPressed =
        (sender, _) => (sender as InputElement)?.Focus();
}