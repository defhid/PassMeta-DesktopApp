using Avalonia.Controls;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;

namespace PassMeta.DesktopApp.Ui.Models.Providers;

/// <inheritdoc />
public class SimpleHostWindowProvider : IHostWindowProvider
{
    /// <inheritdoc />
    public Window Window { get; }

    public SimpleHostWindowProvider(Window hostWindow)
    {
        Window = hostWindow;
    }
}