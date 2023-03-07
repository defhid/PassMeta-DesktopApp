namespace PassMeta.DesktopApp.Common.Models.App;

/// <summary>
/// Information about this application.
/// </summary>
public class AppInfo
{
    /// <summary>
    /// Application author.
    /// </summary>
    public string Author { get; init; } = null!;

    /// <summary>
    /// Copyright text + years.
    /// </summary>
    public string Copyright { get; init; } = null!;

    /// <summary>
    /// Application version.
    /// </summary>
    public string Version { get; init; } = null!;

    /// <summary>
    /// x64/x86.
    /// </summary>
    public string Bit { get; init; } = null!;
    
    /// <summary>
    /// Application data root path.
    /// </summary>
    public string RootPath { get; init; } = null!;
}