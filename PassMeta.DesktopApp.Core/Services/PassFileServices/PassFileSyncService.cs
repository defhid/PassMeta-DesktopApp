using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Core.Services.PassFileServices;

/// <inheritdoc />
public class PassFileSyncService : IPassFileSyncService
{
    private readonly IPassFileRemoteService _pfRemoteService;
    private readonly IDialogService _dialogService;

    /// <summary></summary>
    public PassFileSyncService(IPassFileRemoteService pfRemoteService, IDialogService dialogService)
    {
        _pfRemoteService = pfRemoteService;
        _dialogService = dialogService;
    }

    /// <inheritdoc />
    public async Task SynchronizeAsync<TPassFile>(IPassFileContext<TPassFile> context)
        where TPassFile : PassFile
    {
        var committed = false;
        var synced = false;
        var syncWarning = false;

        if (context.AnyChanged)
        {
            committed |= (await context.CommitAsync()).Ok;
        }
        else
        {
            await context.LoadListAsync();
        }

        var remoteList = await _pfRemoteService.GetListAsync<TPassFile>();
        if (remoteList.Ok)
        {
            syncWarning = !await SynchronizeInternalAsync(context, context.CurrentList, remoteList.Data!);
            synced = true;
        }

        if (context.AnyChanged)
        {
            committed |= (await context.CommitAsync()).Ok;
        }

        if (committed)
        {
            _dialogService.ShowInfo(Resources.PASSCONTEXT__INFO_COMMITED);
        }

        if (synced)
        {
            if (syncWarning)
            {
                _dialogService.ShowFailure(Resources.PASSERVICE__WARN_SYNCHRONIZED,
                    defaultPresenter: DialogPresenter.PopUp);
            }
            else
            {
                _dialogService.ShowInfo(Resources.PASSERVICE__INFO_SYNCHRONIZED);
            }
        }
    }

    private async Task<bool> SynchronizeInternalAsync<TPassFile>(
        IPassFileContext<TPassFile> context,
        IEnumerable<TPassFile> localPassFiles,
        IEnumerable<TPassFile> remotePassFiles)
        where TPassFile : PassFile
    {
        var localList = new LinkedList<TPassFile>(localPassFiles);
        var everythingOk = true;

        foreach (var remote in remotePassFiles)
        {
            var local = localList.FirstOrDefault(pf => pf.Id == remote.Id);
            if (local is null)
            {
                everythingOk &= await OnRemoteCreatedAsync(remote, context);
                continue;
            }

            local.Mark &= ~PassFileMark.AllErrors; // reset errors
            localList.Remove(local);

            if (local.IsLocalDeleted())
            {
                everythingOk &= await OnLocalDeletedAsync(local, remote, context);
                continue;
            }

            if (remote.InfoChangedOn != local.InfoChangedOn)
            {
                everythingOk &= await OnInfoChangedAsync(local, remote, context);
            }

            if (remote.Version != local.Version)
            {
                everythingOk &= await OnVersionChangedAsync(local, remote, context);
            }
        }

        foreach (var local in localList)
        {
            everythingOk &= local.IsLocalCreated()
                ? await OnLocalCreatedAsync(local, context)
                : await OnRemoteDeletedAsync(local, context);
        }

        return everythingOk;
    }

    /// <summary>
    /// Download new passfile.
    /// </summary>
    private async ValueTask<bool> OnRemoteCreatedAsync<TPassFile>(TPassFile remote, IPassFileContext<TPassFile> context)
        where TPassFile : PassFile
    {
        if (!await TryLoadRemoteContent(remote, remote))
        {
            return false;
        }

        return context.Add(remote, null).Ok;
    }

    /// <summary>
    /// Delete passfile finally or restore.
    /// </summary>
    private async ValueTask<bool> OnLocalDeletedAsync<TPassFile>(TPassFile local, TPassFile remote,
        IPassFileContext<TPassFile> context)
        where TPassFile : PassFile
    {
        // restore
        //
        if (local.InfoChangedOn < remote.InfoChangedOn ||
            local.VersionChangedOn < remote.VersionChangedOn)
        {
            if (!await TryLoadRemoteContent(local, remote))
            {
                return false;
            }

            local.DeletedOn = null;
            local.LoadInfoFieldsFrom(remote);

            return context.UpdateInfo(local, true).Ok &&
                   context.UpdateContent(local, true).Ok;
        }

        // delete finally
        //
        var deleteResult = await _pfRemoteService.DeleteAsync(local);
        if (deleteResult.Bad)
        {
            return context.Delete(local, true).Ok;
        }

        local.Mark |= PassFileMark.RemoteDeletingError;
        return false;
    }

    /// <summary>
    /// Apply actual information changes.
    /// </summary>
    private async ValueTask<bool> OnInfoChangedAsync<TPassFile>(TPassFile local, TPassFile remote,
        IPassFileContext<TPassFile> context)
        where TPassFile : PassFile
    {
        if (local.InfoChangedOn > remote.InfoChangedOn)
        {
            if (!await TrySaveRemoteInfo(local))
            {
                return false;
            }
        }
        else
        {
            local.LoadInfoFieldsFrom(remote);
        }

        return context.UpdateInfo(local, true).Ok;
    }

    /// <summary>
    /// Apply actual content changes.
    /// </summary>
    private async ValueTask<bool> OnVersionChangedAsync<TPassFile>(TPassFile local, TPassFile remote,
        IPassFileContext<TPassFile> context)
        where TPassFile : PassFile
    {
        if (local.IsLocalVersionFieldsChanged())
        {
            // apply local
            //
            if (local.OriginChangeStamps!.Version == remote.Version ||
                local.Mark.HasFlag(PassFileMark.Merged))
            {
                var provideResult = await context.ProvideEncryptedContentAsync(local);
                if (provideResult.Bad)
                {
                    return false;
                }

                if (!await TrySaveRemoteContent(local))
                {
                    return false;
                }

                return context.UpdateContent(local, true).Ok;
            }

            // may be conflicts
            //
            local.Mark |= PassFileMark.NeedsMerge;
            _dialogService.ShowFailure(Resources.PASSERVICE__WARN_NEED_MERGE, local.GetTitle());
            return true;
        }

        // apply remote
        //
        if (!await TryLoadRemoteContent(local, remote))
        {
            return false;
        }

        return context.UpdateContent(local, true).Ok;
    }

    /// <summary>
    /// Upload new passfile.
    /// </summary>
    private async ValueTask<bool> OnLocalCreatedAsync<TPassFile>(TPassFile local, IPassFileContext<TPassFile> context)
        where TPassFile : PassFile
    {
        var provideResult = await context.ProvideEncryptedContentAsync(local);
        if (provideResult.Bad)
        {
            return false;
        }

        var addResult = await _pfRemoteService.AddAsync(local);
        if (addResult.Bad)
        {
            local.Mark |= PassFileMark.UploadingError;
            return false;
        }

        var replaceLocal = addResult.Data!;
        replaceLocal.ContentEncrypted = local.ContentEncrypted;
        replaceLocal.LoadVersionFieldsFrom(local);
        
        var okAdd = context.Add(replaceLocal, local).Ok;

        if (!await TrySaveRemoteContent(replaceLocal))
        {
            return false;
        }

        return okAdd && context.UpdateContent(replaceLocal, true).Ok;
    }
    
    /// <summary>
    /// Delete passfile finally.
    /// </summary>
    private ValueTask<bool> OnRemoteDeletedAsync<TPassFile>(TPassFile local, IPassFileContext<TPassFile> context)
        where TPassFile : PassFile
    {
        var ok = context.Delete(local, true).Ok;   
        return ValueTask.FromResult(ok);
    }

    /// <summary>
    /// Load passfile encrypted content from remote.
    /// </summary>
    private async ValueTask<bool> TryLoadRemoteContent<TPassFile>(TPassFile local, TPassFile remote)
        where TPassFile : PassFile
    {
        var result = await _pfRemoteService.GetEncryptedContentAsync(remote.Id, remote.Version);
        if (result.Bad)
        {
            local.Mark |= PassFileMark.DownloadingError;
            return false;
        }

        local.ContentEncrypted = result.Data;
        local.LoadVersionFieldsFrom(remote);
        return true;
    }
    
    /// <summary>
    /// Save passfile encrypted content to remote.
    /// </summary>
    private async ValueTask<bool> TrySaveRemoteContent<TPassFile>(TPassFile passFile)
        where TPassFile : PassFile
    {
        var saveResult = await _pfRemoteService.SaveEncryptedContentAsync(passFile);
        if (saveResult.Bad)
        {
            passFile.Mark |= PassFileMark.UploadingError;
            return false;
        }

        passFile.LoadVersionFieldsFrom(saveResult.Data!);
        return true;
    }
    
    /// <summary>
    /// Save passfile information to remote.
    /// </summary>
    private async ValueTask<bool> TrySaveRemoteInfo<TPassFile>(TPassFile passFile)
        where TPassFile : PassFile
    {
        var saveResult = await _pfRemoteService.SaveInfoAsync(passFile);
        if (saveResult.Bad)
        {
            passFile.Mark |= PassFileMark.UploadingError;
            return false;
        }

        passFile.LoadInfoFieldsFrom(saveResult.Data!);
        return true;
    }
}