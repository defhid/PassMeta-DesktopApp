using PassMeta.DesktopApp.Common.Abstractions.AppContext;

namespace PassMeta.DesktopApp.Common.Abstractions.PassFileContext;

/// <summary>
/// <see cref="IPassFileContext{TPassFile}"/> manager.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IPassFileContextManager : IPassFileContextProvider
{
    /// <summary>
    /// Dispose current contexts and initialize new ones with actual user context.
    /// </summary>
    void Reload(IUserContext userContext);
}