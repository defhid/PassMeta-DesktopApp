using PassMeta.DesktopApp.Common.Abstractions.Utils.Loading;
using PassMeta.DesktopApp.Core.Utils.Loading;

namespace PassMeta.DesktopApp.Core;

/// <summary>
/// Application loading management.
/// </summary>
public static class AppLoading
{
    /// <summary>
    /// General loading.
    /// </summary>
    public static readonly ILoadingManager General = new DefaultLoadingManager();

    /// <summary>
    /// General background loading.
    /// </summary>
    public static readonly ILoadingManager GeneralBackground = new DefaultLoadingManager();

    /// <summary>
    /// Any loading.
    /// </summary>
    public static ILoadingState Any => new CombinedLoadingState(new []
    {
        General,
        GeneralBackground
    });
}