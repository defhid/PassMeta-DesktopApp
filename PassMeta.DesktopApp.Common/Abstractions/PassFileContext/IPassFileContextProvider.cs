using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Abstractions.PassFileContext;

/// <summary>
/// <see cref="IPassFileContext{TPassFile}"/> provider.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IPassFileContextProvider
{
    /// <summary>
    /// Current <see cref="PwdPassFile"/> context.
    /// </summary>
    IPassFileContext<PwdPassFile> PwdPassFileContext { get; }
    
    /// <summary>
    /// Current <see cref="TxtPassFile"/> context.
    /// </summary>
    IPassFileContext<TxtPassFile> TxtPassFileContext { get; }

    /// <summary>
    /// Get <typeparamref name="TPassFile"/> context.
    /// </summary>
    IPassFileContext<TPassFile> PassFileContext<TPassFile>()
        where TPassFile : PassFile;
}