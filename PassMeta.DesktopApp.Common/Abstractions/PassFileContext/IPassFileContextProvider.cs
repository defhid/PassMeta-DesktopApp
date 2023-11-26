using System.Collections.Generic;

namespace PassMeta.DesktopApp.Common.Abstractions.PassFileContext;

/// <summary>
/// <see cref="IPassFileContext{TPassFile}"/> provider.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IPassFileContextProvider
{
    /// <summary>
    /// All currently provided passfile contexts.
    /// </summary>
    IEnumerable<IPassFileContext> Contexts { get; }
}