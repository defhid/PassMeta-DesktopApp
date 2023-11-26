using System.Threading.Tasks;

namespace PassMeta.DesktopApp.Common.Abstractions.App;

/// <summary>
/// Application presets manger.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IAppPresetsManager : IAppPresetsProvider
{
    /// <summary>
    /// Load all presets.
    /// </summary>
    Task LoadAsync();
}