using System;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models.Dto.Response;
using PassMeta.DesktopApp.Common.Models.Entities.Internal;

namespace PassMeta.DesktopApp.Common.Abstractions.AppContext;

/// <summary>
/// <see cref="IAppContext"/> manager.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IAppContextManager : IAppContextProvider
{
    /// <summary>
    /// Load stored context and set it to <see cref="IAppContextProvider.Current"/>.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Refresh context from the server.
    /// </summary>
    Task<IResult> RefreshFromAsync(PassMetaInfoDto passMetaInfoDto);

    /// <summary>
    /// Edit current context model and flush changes.
    /// </summary>
    Task<IResult> ApplyAsync(Action<AppContextModel> setup);
}