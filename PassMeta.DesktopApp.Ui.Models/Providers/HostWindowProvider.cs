using System;
using Avalonia.Controls;

namespace PassMeta.DesktopApp.Ui.Models.Providers;

/// <summary>
/// Host window provider.
/// </summary>
public sealed class HostWindowProvider : IDisposable
{
    /// <summary>
    /// Current host window.
    /// </summary>
    public Window? Window { get; private set; }

    public HostWindowProvider(Window hostWindow)
    {
        Window = hostWindow;
    }

    private HostWindowProvider()
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Window = null;
    }

    public static readonly HostWindowProvider Empty = new();
}