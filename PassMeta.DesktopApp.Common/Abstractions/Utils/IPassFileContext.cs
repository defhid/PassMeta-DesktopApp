using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Entities.PassFile;
using PassMeta.DesktopApp.Common.Enums;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils;

/// <summary>
/// A context for working with passfiles via local storage.
/// </summary>
/// <remarks>Stateful, user-scoped.</remarks>
public interface IPassFileContext<TPassFile> : IDisposable
    where TPassFile : IPassFile
{
    /// <summary>
    /// Has any uncommited added/changed/deleted passfile?
    /// </summary>
    bool AnyCurrentChanged { get; }

    /// <summary>
    /// Represents <see cref="AnyCurrentChanged"/>.
    /// </summary>
    IObservable<bool> AnyCurrentChangedSource { get; }

    /// <summary>
    /// Get passfile list loaded since the last <see cref="LoadAsync"/> method was called.
    /// Reflects uncommitted state, changed passfiles have a higher priority.
    /// </summary>
    IEnumerable<TPassFile> CurrentList { get; }

    /// <summary>
    /// Load passfile list by current application context.
    /// </summary>
    Task LoadAsync(IAppContext appContext);

    /// <summary>
    /// Save all changes.
    /// </summary>
    Task<IResult> CommitAsync();

    /// <summary>
    /// Rollback all changes.
    /// </summary>
    void Rollback();

    #region Create / Update / Delete

    /// <summary>
    /// Create a new local passfile with id based on sequence from <see cref="ICounter"/>,
    /// add to the current list and return it.
    /// </summary>
    TPassFile CreateNew(PassFileType ofType);

    /// <summary>
    /// Add a new passfile from remote.
    /// </summary>
    IResult Add(TPassFile actualPassFile);

    /// <summary>
    /// Detect changes, mark passfile as changed and prepare it to commit.
    /// </summary>
    /// <remarks>
    /// If <paramref name="passFileId"/> does not math the id from <paramref name="actualPassFile"/>,
    /// it will be replaced with the actual.
    /// </remarks>
    IResult UpdateInfo(int passFileId, IPassFile actualPassFile);

    /// <summary>
    /// Mark passfile as changed and prepare it to commit.
    /// </summary>
    /// <remarks>
    /// If <paramref name="passFileId"/> does not math the id from <paramref name="actualPassFile"/>,
    /// it will be replaced with the actual.
    /// </remarks>
    IResult UpdateContentEncrypted(TPassFile actualPassFile, byte[] content);

    /// <summary>
    /// Mark passfile as deleted, exclude from <see cref="CurrentList"/> and prepare it to commit.
    /// </summary>
    IResult Delete(int passFileId);

    #endregion

    IResult<byte[]> Encrypt(TPassFile passFile);

    IResult Decrypt(TPassFile passFile);
}