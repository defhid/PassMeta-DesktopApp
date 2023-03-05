using PassMeta.DesktopApp.Common.Models.App;
using PassMeta.DesktopApp.Core.Utils.Loading;

namespace PassMeta.DesktopApp.Core;

/// <summary>
/// Factory for <see cref="AppLoading"/>.
/// </summary>
public static class AppLoadingFactory
{
    /// <summary>
    /// Create a new <see cref="AppLoading"/>.
    /// </summary>
    public static AppLoading Create() => new()
    {
        General = new DefaultLoadingManager(),
    };
}