using PassMeta.DesktopApp.Common.Models.Presets;

namespace PassMeta.DesktopApp.Common.Abstractions.App;

/// <summary>
/// Application presets provider.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IAppPresetsProvider
{
    /// <summary>
    /// Current <see cref="Models.Presets.PasswordGeneratorPresets"/>.
    /// </summary>
    PasswordGeneratorPresets PasswordGeneratorPresets { get; }
}