using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils;

/// <summary>
/// A context for working with passfiles via local storage.
/// </summary>
/// <remarks>Stateful, user-scoped.</remarks>
public interface IPassFileContext : IDisposable
{
    /// <summary>
    /// Supported passfile type.
    /// </summary>
    PassFileType PassFileType { get; }
    
    /// <summary>
    /// Has any added/changed/deleted passfile?
    /// </summary>
    bool AnyChanged { get; }

    /// <summary>
    /// Represents <see cref="AnyChanged"/>.
    /// </summary>
    IObservable<bool> AnyChangedSource { get; }

    /// <summary>
    /// Save all changes.
    /// </summary>
    Task<IResult> CommitAsync();

    /// <summary>
    /// Rollback all changes.
    /// </summary>
    void Rollback();
}

/// <inheritdoc />
public interface IPassFileContext<TPassFile> : IPassFileContext
    where TPassFile : PassFile
{
    /// <summary>
    /// Get current passfile list.
    /// </summary>
    IEnumerable<TPassFile> CurrentList { get; }

    /// <summary>
    /// Load passfile content of its current version.
    /// </summary>
    Task<IResult> LoadContentAsync(TPassFile passFile);

    /// <summary>
    /// Create a new passfile with local id, add it to <see cref="CurrentList"/>.
    /// </summary>
    TPassFile Create();

    /// <summary>
    /// Add <see cref="originPassFile"/> to <see cref="CurrentList"/>,
    /// remove <paramref name="replacePassFile"/> from <see cref="CurrentList"/>
    /// and reassign contents to the origin passfile.
    /// </summary>
    IResult<TPassFile> Add(TPassFile originPassFile, TPassFile? replacePassFile);

    /// <summary>
    /// Mark passfile as information-changed.
    /// </summary>
    /// <remarks>Parameter <paramref name="passFile"/> must be from <see cref="CurrentList"/>.</remarks>
    IResult UpdateInfo(TPassFile passFile, bool fromOrigin);

    /// <summary>
    /// Mark passfile as version-changed.
    /// </summary>
    /// <remarks>Parameter <paramref name="passFile"/> must be from <see cref="CurrentList"/>.</remarks>
    IResult UpdateContent(TPassFile passFile, bool fromOrigin);

    /// <summary>
    /// Mark passfile as deleted, exclude from <see cref="CurrentList"/>.
    /// </summary>
    IResult Delete(int passFileId, bool fromOrigin);
}