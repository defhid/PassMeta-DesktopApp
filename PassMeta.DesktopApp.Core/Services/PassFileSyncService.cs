using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
    
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Services;

/// <inheritdoc />
public class PassFileSyncService : IPassFileSyncService
{
    private readonly IPassFileRemoteService _remoteService;
    private readonly IPassMetaClient _passMetaClient;
    private readonly IDialogService _dialogService;
    private readonly ILogService _logger;

    /// <summary></summary>
    public PassFileSyncService(
        IPassFileRemoteService remoteService,
        IPassMetaClient passMetaClient,
        IDialogService dialogService,
        ILogService logger)
    {
        _remoteService = remoteService;
        _passMetaClient = passMetaClient;
        _dialogService = dialogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task RefreshLocalPassFilesAsync(PassFileType passFileType)
    {
        if (!_passMetaClient.Online) return;

        var remoteList = await _remoteService.GetListAsync(passFileType);
        if (remoteList is null) return;
            
        var localList = PassFileManager.GetCurrentList(passFileType);

        await _SynchronizeAsync(localList, remoteList);

        if (PassFileManager.AnyCurrentChanged)
        {
            var commitResult = await PassFileManager.CommitAsync(passFileType);
            
            if (commitResult.Ok)
                _dialogService.ShowInfo(Resources.PASSERVICE__INFO_COMMITED);
            else
                _dialogService.ShowError(commitResult.Message!);
        }
    }

    /// <inheritdoc />
    public async Task ApplyPassFileLocalChangesAsync(PassFileType passFileType)
    {
        var committed = false;
        var synced = false;
            
        if (PassFileManager.AnyCurrentChanged)
        {
            var commitResult = await PassFileManager.CommitAsync(passFileType);
            committed |= commitResult.Ok;

            if (commitResult.Bad)
                _dialogService.ShowError(commitResult.Message!);
        }
            
        if (await _passMetaClient.CheckConnectionAsync())
        {
            var remoteList = await _remoteService.GetListAsync(passFileType);
            if (remoteList is not null)
            {
                var localList = PassFileManager.GetCurrentList(passFileType);
                await _SynchronizeAsync(localList, remoteList);
                synced = true;
            }
        }

        if (synced && PassFileManager.AnyCurrentChanged)
        {
            var commitResult = await PassFileManager.CommitAsync(passFileType);
            committed |= commitResult.Ok;

            if (commitResult.Bad)
                _dialogService.ShowError(commitResult.Message!);
        }

        if (committed)
            _dialogService.ShowInfo(Resources.PASSERVICE__INFO_COMMITED);
    }

    private async Task _SynchronizeAsync(IEnumerable<PassFile> localPassFiles, IEnumerable<PassFile> remotePassFiles)
    {
        var localList = new LinkedList<PassFile>(localPassFiles);
            
        foreach (var remote in remotePassFiles)
        {
            var local = localList.FirstOrDefault(pf => pf.Id == remote.Id);
                
            if (local is not null)
                localList.Remove(local);

            PassFileManager.TryResetProblem(remote.Id);

            if (local is null)
            {
                if (await _TryLoadRemoteEncryptedAsync(remote, remote.Version))
                {
                    CheckAsDownloading(remote, PassFileManager.AddFromRemote(remote));
                }
                    
                continue;
            }
                
            if (local.IsLocalDeleted())
            {
                if (local.InfoChangedOn < remote.InfoChangedOn ||
                    local.VersionChangedOn < remote.VersionChangedOn)
                {
                    if (CheckAsDownloading(remote, PassFileManager.UpdateInfo(remote))
                        && await _TryLoadRemoteEncryptedAsync(remote, remote.Version))
                    {
                        CheckAsDownloading(remote, PassFileManager.UpdateData(remote));
                    }
                }
                else
                {
                    await _TryDeleteAsync(local);
                }

                continue;
            }

            if (local.IsLocalChanged())
            {
                PassFile actual;

                if (local.InfoChangedOn > remote.InfoChangedOn)
                {
                    var res = Result.FromResponse(await _remoteService.SaveInfoAsync(local));
                    if (CheckAsUploading(local, res))
                    {
                        actual = res.Data!;
                        CheckAsDownloading(actual, PassFileManager.UpdateInfo(actual, true));
                    }
                    else actual = local;
                }
                else if (local.InfoChangedOn < remote.InfoChangedOn)
                {
                    actual = remote.Copy(false);
                    CheckAsDownloading(actual, PassFileManager.UpdateInfo(actual, true));
                }
                else actual = local;

                actual.RefreshDataFieldsFrom(local, true);

                if (local.IsVersionChanged())
                {
                    if (local.OriginChangeStamps!.Version != remote.Version && !local.Marks.HasFlag(PassFileMark.Merged))
                    {
                        PassFileManager.TrySetProblem(actual.Id, PassFileProblemKind.NeedsMerge);
                        _dialogService.ShowFailure(Resources.PASSERVICE__WARN_NEED_MERGE, actual.GetTitle(), 
                            defaultPresenter: DialogPresenter.PopUp);
                    }
                    else
                    {
                        if (CheckAsUploading(actual, await _EnsureHasLocalEncryptedAsync(actual)))
                        {
                            var res = Result.FromResponse(await _remoteService.SaveContentAsync(actual));
                            if (CheckAsUploading(actual, res))
                            {
                                PassFileManager.TryResetProblem(actual.Id);
                                actual.RefreshDataFieldsFrom(res.Data!.WithEncryptedDataFrom(local), false);

                                CheckAsDownloading(actual, PassFileManager.UpdateData(actual, true));
                            }
                        }
                    }
                }
                else if (local.Version != remote.Version)
                {
                    if (await _TryLoadRemoteEncryptedAsync(actual, remote.Version))
                    {
                        CheckAsDownloading(actual, PassFileManager.UpdateData(actual, true));
                    }
                }
                    
                continue;
            }

            if (remote.InfoChangedOn != local.InfoChangedOn)
            {
                CheckAsDownloading(remote, PassFileManager.UpdateInfo(remote, true));
            }

            if (remote.Version != local.Version)
            {
                if (await _TryLoadRemoteEncryptedAsync(remote, remote.Version))
                {
                    CheckAsDownloading(remote, PassFileManager.UpdateData(remote, true));
                }
            }
        }
            
        foreach (var local in localList)
        {
            if (local.IsLocalCreated()) await _TryAddPassFileAsync(local);
            else PassFileManager.Delete(local, true);
        }
            
        _dialogService.ShowInfo(Resources.PASSERVICE__INFO_SYNCHRONIZED);
    }

    private async Task _TryAddPassFileAsync(PassFile passFile)
    {
        if (CheckAsUploading(passFile, await _EnsureHasLocalEncryptedAsync(passFile)))
        {
            var result = await _remoteService.AddAsync(passFile);  // + save content
            if (result.Ok)
            {
                _logger.Info($"{passFile} created on the server");

                var actual = result.Data!.WithEncryptedContentFrom(passFile);

                CheckAsDownloading(actual, PassFileManager.AddFromRemote(actual, passFile.Id));
            }
        }
    }

    private async Task _TryDeleteAsync(PassFile passFile)
    {
        var answer = await _dialogService.AskPasswordAsync(string.Format(
            Resources.PASSERVICE__ASK_PASSWORD_TO_DELETE_PASSFILE, 
            passFile.Name, passFile.Id, passFile.LocalDeletedOn?.ToShortDateTimeString()));

        if (answer.Bad) return;

        var response = await _remoteService.DeleteAsync(passFile, answer.Data!);
        if (response?.Success is true)
        {
            _logger.Info($"{passFile} deleted from the server");
            PassFileManager.Delete(passFile, true);
        }
        else
        {
            PassFileManager.TrySetProblem(passFile.Id, 
                PassFileProblemKind.RemoteDeletingError.ToProblemWithInfo(response.GetFullMessage()));
        }
    }

    private async Task<bool> _TryLoadRemoteEncryptedAsync(PassFile passFile, int version)
    {
        var data = await _remoteService.GetContentAsync(passFile.Id, version);
        if (data is null)
        {
            PassFileManager.TrySetProblem(passFile.Id, PassFileProblemKind.DownloadingError);
            return false;
        }

        passFile.ContentEncrypted = data;
        return true;
    }
        
    private async Task<IDetailedResult> _EnsureHasLocalEncryptedAsync(PassFile passFile)
    {
        if (passFile.ContentEncrypted is null)
        {
            var result = await PassFileManager.GetEncryptedDataAsync(passFile.Type, passFile.Id);
                
            var res = EnsureOk(passFile, result);
            if (res.Bad) return res;

            passFile.ContentEncrypted = result.Data;
        }

        return Result.Success();
    }

    #region Checking

    private bool CheckAsDownloading(PassFile passFile, IDetailedResult result)
    {
        var res = EnsureOk(passFile, result);
        if (res.Bad)
        {
            PassFileManager.TrySetProblem(passFile.Id,
                PassFileProblemKind.DownloadingError.ToProblemWithInfo(res.Message));
        }

        return res.Ok;
    }
        
    private bool CheckAsUploading(PassFile passFile, IDetailedResult result)
    {
        var res = EnsureOk(passFile, result);
        if (res.Bad)
        {
            PassFileManager.TrySetProblem(passFile.Id,
                PassFileProblemKind.UploadingError.ToProblemWithInfo(res.Message));
        }

        return res.Ok;
    }

    private IDetailedResult EnsureOk(PassFile passFile, IDetailedResult result)
    {
        if (result.Bad)
            _dialogService.ShowError(result.Message!, passFile.GetTitle());

        return result;
    }

    #endregion
}