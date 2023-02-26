using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Abstractions.PassFileContext;

/// <summary>
/// A context for working with passfiles via local storage.
/// </summary>
/// <remarks>Stateful. Scoped (by user).</remarks>
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
    /// <remarks>If result is bad, message will be shown by dialog service.</remarks>
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
    /// Load <see cref="CurrentList"/> if it hasn't been loaded yet,
    /// otherwise execute <see cref="IPassFileContext.Rollback"/>.
    /// </summary>
    /// <remarks>If result is bad, message will be shown by dialog service.</remarks>
    ValueTask<IResult> LoadListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Load passfile encrypted content of its current version.
    /// </summary>
    /// <remarks>If result is bad, message will be shown by dialog service.</remarks>
    ValueTask<IResult> LoadEncryptedContentAsync(TPassFile passFile, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Load passfile encrypted content of its current version
    /// or encrypt current decrypted content.
    /// </summary>
    /// <remarks>If result is bad, message will be shown by dialog service.</remarks>
    ValueTask<IResult> ProvideEncryptedContentAsync(TPassFile passFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new passfile with local id, add it to <see cref="CurrentList"/>.
    /// </summary>
    ValueTask<TPassFile> CreateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Add <paramref name="originPassFile"/> to <see cref="CurrentList"/>,
    /// remove <paramref name="replacePassFile"/> from <see cref="CurrentList"/>
    /// and reassign contents to the origin passfile.
    /// </summary>
    /// <remarks>If result is bad, message will be shown by dialog service.</remarks>
    IResult Add(TPassFile originPassFile, TPassFile? replacePassFile);

    /// <summary>
    /// Mark passfile as information-changed.
    /// </summary>
    /// <remarks>
    /// Parameter <paramref name="passFile"/> must be from <see cref="CurrentList"/>.
    /// If result is bad, message will be shown by dialog service.
    /// </remarks>
    IResult UpdateInfo(TPassFile passFile, bool fromOrigin = false);

    /// <summary>
    /// Mark passfile as version-changed.
    /// </summary>
    /// <remarks>
    /// Parameter <paramref name="passFile"/> must be from <see cref="CurrentList"/>.
    /// If result is bad, message will be shown by dialog service.
    /// </remarks>
    IResult UpdateContent(TPassFile passFile, bool fromOrigin = false);

    /// <summary>
    /// Mark passfile as deleted.
    /// </summary>
    /// <remarks>If result is bad, message will be shown by dialog service.</remarks>
    IResult Delete(TPassFile passFile, bool fromOrigin = false);

    /// <summary>
    /// Mark passfile as restored after local deletion.
    /// </summary>
    /// <remarks>If result is bad, message will be shown by dialog service.</remarks>
    IResult Restore(TPassFile passFile);
}