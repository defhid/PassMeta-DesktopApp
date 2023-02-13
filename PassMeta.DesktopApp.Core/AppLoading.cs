using System.Collections.Generic;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Loading;
using PassMeta.DesktopApp.Core.Utils.Loading;

namespace PassMeta.DesktopApp.Core;

/// <summary>
/// Application loading management.
/// </summary>
public static class AppLoading
{
    private static readonly List<ILoadingState> _loadingStates = new();

    /// <summary>
    /// Begin tracking loading state.
    /// </summary>
    public static void Observe(ILoadingState loadingManager)
    {
        _loadingStates.Add(loadingManager);
    }

    /// <summary>
    /// Finish tracking loading state.
    /// </summary>
    public static void Release(ILoadingState loadingManager)
    {
        _loadingStates.Remove(loadingManager);
    }
    
    public static ILoadingState[] CurrentList { get; }

    /// <summary>
    /// Any loading.
    /// </summary>
    public static ILoadingState Any => new CombinedLoadingState(_loadingStates);
}