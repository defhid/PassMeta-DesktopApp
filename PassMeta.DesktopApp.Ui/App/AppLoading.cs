using PassMeta.DesktopApp.Common.Abstractions.Utils.Loading;
using PassMeta.DesktopApp.Core.Utils.Loading;

namespace PassMeta.DesktopApp.Ui.App;

/// <summary>
/// Application loading management.
/// </summary>
public static class AppLoading
{
    /// <summary>
    /// General loading manager.
    /// </summary>
    public static readonly ILoadingManager General = new DefaultLoadingManager();
}