using System;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models.Internal;

namespace PassMeta.DesktopApp.Common.Abstractions.App;

/// <summary>
/// <see cref="IAppConfig"/> manager.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IAppConfigManager : IAppConfigProvider
{
    /// <summary>
    /// Load stored config and set it to <see cref="IAppConfigProvider.Current"/>.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Edit current config model and flush changes.
    /// </summary>
    Task<IDetailedResult> ApplyAsync(Action<AppConfigModel> setup);
}