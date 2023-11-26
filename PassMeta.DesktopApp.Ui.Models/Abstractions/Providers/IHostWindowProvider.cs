using Avalonia.Controls;

namespace PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;

/// <summary>
/// Host window provider.
/// </summary>
public interface IHostWindowProvider
{
    /// <summary>
    /// Current host window.
    /// </summary>
    public Window Window { get; }
}