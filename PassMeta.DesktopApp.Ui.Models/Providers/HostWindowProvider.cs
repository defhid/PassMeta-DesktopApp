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

    /// <summary></summary>
    public HostWindowProvider(Window hostWindow)
    {
        Window = hostWindow;
    }

    /// <summary></summary>
    private HostWindowProvider()
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Window = null;
    }

    /// <summary></summary>
    public static readonly HostWindowProvider Empty = new();
}