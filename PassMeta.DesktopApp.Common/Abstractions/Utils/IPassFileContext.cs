using System;
using System.Collections.Generic;
using System.Threading;
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
    /// Load <see cref="CurrentList"/>.
    /// </summary>
    Task<IResult> LoadListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Load passfile content of its current version.
    /// </summary>
    Task<IResult> LoadContentAsync(TPassFile passFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new passfile with local id, add it to <see cref="CurrentList"/>.
    /// </summary>
    Task<TPassFile> CreateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Add <see cref="originPassFile"/> to <see cref="CurrentList"/>,
    /// remove <paramref name="replacePassFile"/> from <see cref="CurrentList"/>
    /// and reassign contents to the origin passfile.
    /// </summary>
    IResult Add(TPassFile originPassFile, TPassFile? replacePassFile);

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
    /// Mark passfile as deleted.
    /// </summary>
    IResult Delete(TPassFile passFile, bool fromOrigin);

    /// <summary>
    /// Mark passfile as restored after local deletion.
    /// </summary>
    IResult Restore(TPassFile passFile);
}