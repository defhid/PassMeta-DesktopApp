namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common;
    using DesktopApp.Common.Enums;
    using DesktopApp.Common.Interfaces;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Interfaces.Services.PassFile;
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using DesktopApp.Common.Utils.Extensions;

    using DesktopApp.Core.Utils;
    using DesktopApp.Core.Utils.Extensions;
    
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Models.Entities.Extra;

    /// <inheritdoc />
    public class PassFileSyncService : IPassFileSyncService
    {
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();
        private readonly ILogService _logger = EnvironmentContainer.Resolve<ILogService>();
        private readonly IPassFileRemoteService _remoteService = EnvironmentContainer.Resolve<IPassFileRemoteService>();

        /// <inheritdoc />
        public async Task RefreshLocalPassFilesAsync(PassFileType passFileType)
        {
            if (!PassMetaApi.Online) return;

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
            
            if (await PassMetaApi.CheckConnectionAsync())
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
                
                if (local.LocalDeleted)
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

                if (local.LocalChanged)
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
                        if (local.Origin!.Version != remote.Version && !local.Marks.HasFlag(PassFileMark.Merged))
                        {
                            PassFileManager.TrySetProblem(actual.Id, PassFileProblemKind.NeedsMerge);
                            _dialogService.ShowFailure(Resources.PASSERVICE__WARN_NEED_MERGE, actual.GetTitle(), 
                                defaultPresenter: DialogPresenter.PopUp);
                        }
                        else
                        {
                            if (CheckAsUploading(actual, await _EnsureHasLocalEncryptedAsync(actual)))
                            {
                                var res = Result.FromResponse(await _remoteService.SaveDataAsync(actual));
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
                if (local.LocalCreated) await _TryAddPassFileAsync(local);
                else PassFileManager.Delete(local, true);
            }
            
            _dialogService.ShowInfo(Resources.PASSERVICE__INFO_SYNCHRONIZED);
        }

        private async Task _TryAddPassFileAsync(PassFile passFile)
        {
            if (CheckAsUploading(passFile, await _EnsureHasLocalEncryptedAsync(passFile)))
            {
                var response = await _remoteService.AddAsync(passFile);
                if (response?.Success is true)
                {
                    _logger.Info($"{passFile} created on the server");

                    var actual = response.Data!.WithEncryptedDataFrom(passFile);

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
            var result = Result.FromResponse(await _remoteService.GetDataAsync(passFile.Id, version));

            if (!CheckAsDownloading(passFile, result)) return false;
            
            passFile.DataEncrypted = result.Data!;
            return true;
        }
        
        private async Task<IDetailedResult> _EnsureHasLocalEncryptedAsync(PassFile passFile)
        {
            if (passFile.DataEncrypted is null)
            {
                var result = await PassFileManager.GetEncryptedDataAsync(passFile.Type, passFile.Id);
                
                var res = EnsureOk(passFile, result);
                if (res.Bad) return res;

                passFile.DataEncrypted = result.Data;
            }

            return Result.Success();
        }

        #region Checking

        private bool CheckAsDownloading(PassFile passFile, IDetailedResult result)
        {
            var res = EnsureOk(passFile, result);
            if (res.Ok)
            {
                _logger.Info($"{passFile} downloaded from the server");
            }
            else
            {
                PassFileManager.TrySetProblem(passFile.Id,
                    PassFileProblemKind.DownloadingError.ToProblemWithInfo(res.Message));
            }

            return res.Ok;
        }
        
        private bool CheckAsUploading(PassFile passFile, IDetailedResult result)
        {
            var res = EnsureOk(passFile, result);
            if (res.Ok)
            {
                _logger.Info($"{passFile} uploaded to the server");
            }
            else
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
}