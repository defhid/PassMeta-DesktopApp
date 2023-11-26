using System;
using Avalonia.Controls;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;

namespace PassMeta.DesktopApp.Ui.Models.Providers;

/// <summary>
/// Host window provider for previews.
/// </summary>
public class PreviewHostWindowProvider : IHostWindowProvider
{
    /// <inheritdoc />
    public Window Window => throw new InvalidOperationException("Host window does not exist for preview");

    private PreviewHostWindowProvider()
    {
    }

    public static readonly PreviewHostWindowProvider Instance = new();
}