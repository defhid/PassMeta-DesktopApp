using PassMeta.DesktopApp.Common.Abstractions.Utils.Loading;

namespace PassMeta.DesktopApp.Common.Models.App;

/// <summary>
/// Application loading management.
/// </summary>
public class AppLoading
{
    /// <summary></summary>
    public AppLoading(ILoadingManager general)
    {
        General = general;
    }

    /// <summary>
    /// General loading manager.
    /// </summary>
    public ILoadingManager General { get; }
}