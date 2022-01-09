namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common;
    using DesktopApp.Common.Enums;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using DesktopApp.Common.Utils.Mapping;
    using DesktopApp.Core.Utils;
    using DesktopApp.Core.Utils.Extensions;
    
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Splat;

    /// <inheritdoc />
    public class PassFileService : IPassFileService
    {
        private static readonly ToStringMapper<string> WhatToStringMapper = new MapToResource<string>[]
        {
            new("passfile_id", () => Resources.DICT_STORAGE__PASSFILE_ID),
            new("name", () => Resources.DICT_STORAGE__PASSFILE_NAME),
            new("color", () => Resources.DICT_STORAGE__PASSFILE_COLOR),
            new("created_on", () => Resources.DICT_STORAGE__PASSFILE_CREATED_ON),
            new("smth", () => Resources.DICT_STORAGE__PASSFILE_SMTH),
            new("check_password", () => Resources.DICT_STORAGE__CHECK_PASSWORD)
        };
        
        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;

        /// <inheritdoc />
        public async Task RefreshLocalPassFilesAsync()
        {
            if (!PassMetaApi.Online) return;

            var remoteList = await _GetPassFileListRemoteAsync();
            if (remoteList is null) return;
            
            var localList = PassFileManager.GetCurrentList();

            foreach (var remote in remoteList)
            {
                var local = localList.FirstOrDefault(pf => pf.Id == remote.Id);
                if (local is not null)
                    localList.Remove(local);

                if (local is null)
                {
                    var response = await _GetPassFileDataRemoteAsync(remote.Id);
                    
                    if (response?.Data is null)
                    {
                        PassFileManager.TrySetProblem(remote.Id, 
                            PassFileProblemKind.DownloadingError.ToProblemWithInfo(response?.Message));
                    }
                    else
                    {
                        remote.DataEncrypted = response.Data!;
                        
                        var result = PassFileManager.AddFromRemote(remote);
                        if (result.Bad)
                            _dialogService.ShowError(result.Message!, remote.GetTitle());
                    }
                    
                    continue;
                }
                
                if (local.LocalDeleted)
                {
                    if (local.InfoChangedOn < remote.InfoChangedOn ||
                        local.VersionChangedOn < remote.VersionChangedOn)
                    {
                        PassFileManager.UpdateInfo(remote);
                        
                        var response = await _GetPassFileDataRemoteAsync(remote.Id);
                        if (response?.Data is null)
                        {
                            PassFileManager.TrySetProblem(remote.Id, 
                                PassFileProblemKind.DownloadingError.ToProblemWithInfo(response.GetLocalizedMessage()));
                        }
                        else
                        {
                            remote.DataEncrypted = response.Data!;
                            
                            var result = PassFileManager.UpdateData(remote);
                            if (result.Bad)
                                _dialogService.ShowError(result.Message!, remote.GetTitle());
                        }
                    }
                    else
                    {
                        var answer = await _dialogService.AskPasswordAsync(string.Format(
                            Resources.PASSERVICE__ASK_PASSWORD_TO_DELETE_PASSFILE, 
                            remote.Name, remote.Id, local.InfoChangedOn.ToShortDateString()));
                        
                        if (answer.Ok)
                        {
                            var response = await _DeletePassFileRemoteAsync(remote, answer.Data!);
                            if (response?.Success is true)
                            {
                                PassFileManager.Delete(local);
                            }
                            else
                            {
                                PassFileManager.TrySetProblem(local.Id, PassFileProblemKind.RemoteDeletingError);
                            }
                        }
                    }
                    
                    continue;
                }

                if (local.LocalChanged)
                {
                    PassFile actual;
                    
                    #region Info
                    
                    if (local.InfoChangedOn > remote.InfoChangedOn)
                    {
                        actual = local;
                        var response = await _SavePassFileInfoRemoteAsync(actual);
                        if (response?.Success is true)
                        {
                            actual = response.Data!;
                        }
                        else
                        {
                            PassFileManager.TrySetProblem(actual.Id,
                                PassFileProblemKind.UploadingError.ToProblemWithInfo(response.GetLocalizedMessage()));
                        }
                    }
                    else if (local.InfoChangedOn < remote.InfoChangedOn) actual = remote;
                    else actual = local;

                    var result = PassFileManager.UpdateInfo(actual, true);
                    if (result.Bad)
                        _dialogService.ShowError(result.Message!, actual.GetTitle());

                    #endregion
                    
                    #region Version
                    
                    if (local.IsVersionChanged())
                    {
                        if (local.Origin!.Version != remote.Version)
                        {
                            PassFileManager.TrySetProblem(actual.Id, PassFileProblemKind.NeedsMerge);
                        }
                        else
                        {
                            var res = await PassFileManager.GetEncryptedDataAsync(actual.Id);
                            if (res.Ok)
                            {
                                actual.DataEncrypted = res.Data!;
                                var push = await _SavePassFileDataRemoteAsync(actual);
                                if (push?.Success is not true)
                                {
                                    PassFileManager.TrySetProblem(actual.Id,
                                        PassFileProblemKind.UploadingError.ToProblemWithInfo(push.GetLocalizedMessage()));
                                }
                            }
                            else
                            {
                                PassFileManager.TrySetProblem(actual.Id,
                                    PassFileProblemKind.UploadingError.ToProblemWithInfo(res.Message));
                                _dialogService.ShowError(res.Message!, actual.GetTitle());
                            }
                        }
                    }
                    else if (local.Version != remote.Version)
                    {
                        var response = await _GetPassFileDataRemoteAsync(actual.Id);
                        if (response?.Success is true)
                        {
                            actual.DataEncrypted = response.Data!;

                            var res = PassFileManager.UpdateData(actual, true);
                            if (res.Bad)
                                _dialogService.ShowError(res.Message!, actual.GetTitle());
                        }
                        else
                        {
                            PassFileManager.TrySetProblem(actual.Id,
                                PassFileProblemKind.DownloadingError.ToProblemWithInfo(response.GetLocalizedMessage()));
                        }
                    }
                    
                    #endregion
                    
                    continue;
                }

                if (remote.InfoChangedOn != local.InfoChangedOn)
                {
                    var result = PassFileManager.UpdateInfo(remote, true);
                    if (result.Bad)
                        _dialogService.ShowError(result.Message!, remote.GetTitle());
                }

                if (remote.Version != local.Version)
                {
                    var response = await _GetPassFileDataRemoteAsync(remote.Id);
                    if (response?.Success is true)
                    {
                        remote.DataEncrypted = response.Data!;

                        var res = PassFileManager.UpdateData(remote, true);
                        if (res.Bad)
                            _dialogService.ShowError(res.Message!);
                    }
                    else
                    {
                        PassFileManager.TrySetProblem(remote.Id, 
                             PassFileProblemKind.DownloadingError.ToProblemWithInfo(response.GetLocalizedMessage()));
                    }
                }
            }
            
            foreach (var local in localList)
            {
                PassFileManager.Delete(local);
            }

            var commitResult = await PassFileManager.CommitAsync();
            if (commitResult.Bad)
                _dialogService.ShowError(commitResult.Message!);
        }

        /// <inheritdoc />
        public async Task ApplyPassFileLocalChangesAsync()
        {
            var list = PassFileManager.GetCurrentList();

            if (PassMetaApi.Online)
            {
                foreach (var passFile in list)
                {
                    if (passFile.LocalCreated)
                    {
                        var response = await _AddPassFileRemoteAsync(passFile);
                        if (response?.Success is true)
                        {
                            var res = PassFileManager.AddFromRemote(response.Data!);
                            if (res.Bad)
                                _dialogService.ShowError(res.Message!, passFile.GetTitle());
                            
                            PassFileManager.Delete(passFile, true);
                        }
                    }
                    else if (passFile.LocalChanged)
                    {
                        if (passFile.IsInformationChanged())
                        {
                            var response = await _SavePassFileInfoRemoteAsync(passFile);
                            if (response?.Success is true)
                            {
                                PassFileManager.UpdateInfo(response.Data!, true);
                            }
                        }

                        if (passFile.IsVersionChanged())
                        {
                            if (passFile.DataEncrypted is null)
                            {
                                var res = await PassFileManager.GetEncryptedDataAsync(passFile.Id);
                                if (res.Bad)
                                    _dialogService.ShowError(res.Message!, passFile.GetTitle());

                                passFile.DataEncrypted = res.Data;
                            }

                            if (passFile.DataEncrypted is not null)
                            {
                                var response = await _SavePassFileDataRemoteAsync(passFile);
                                if (response?.Success is true)
                                {
                                    var res = PassFileManager.UpdateData(response.Data!, true);
                                    if (res.Bad)
                                        _dialogService.ShowError(res.Message!, passFile.GetTitle());
                                }
                            }
                        }
                    }
                    else if (passFile.LocalDeleted)
                    {
                        var answer = await _dialogService.AskPasswordAsync(string.Format(
                            Resources.PASSERVICE__ASK_PASSWORD_TO_DELETE_PASSFILE, 
                            passFile.Name, passFile.Id, passFile.InfoChangedOn.ToShortDateString()));
                        
                        if (answer.Ok)
                        {
                            await _DeletePassFileRemoteAsync(passFile, answer.Data!);
                        }
                    }
                }
            }

            var commitResult = await PassFileManager.CommitAsync();
            
            if (commitResult.Ok)
                _dialogService.ShowInfo(Resources.PASSERVICE__SUCCESS_COMMIT);
            else
                _dialogService.ShowError(commitResult.Message!);
        }

        #region Api Requests

        private static async Task<List<PassFile>?> _GetPassFileListRemoteAsync()
        {
            var response = await PassMetaApi.GetAsync<List<PassFile>>("/passfiles/list", true);
            return response?.Data;
        }

        private static Task<OkBadResponse<string>?> _GetPassFileDataRemoteAsync(int passFileId, int? version = null)
        {
            var url = version is null
                ? $"/passfiles/{passFileId}/smth"
                : $"/passfiles/{passFileId}/smth?version={version}";
            
            return PassMetaApi.GetAsync<string>(url, true);
        }

        private static Task<OkBadResponse<PassFile>?> _SavePassFileInfoRemoteAsync(PassFile passFile)
        {
            var request = PassMetaApi.Patch($"/passfiles/{passFile.Id}/info", passFile);

            return request.WithContext(passFile.GetTitle())
                .WithBadHandling(WhatToStringMapper)
                .ExecuteAsync<PassFile>();
        }
        
        private static Task<OkBadResponse<PassFile>?> _SavePassFileDataRemoteAsync(PassFile passFile)
        {
            var request = PassMetaApi.Patch($"/passfiles/{passFile.Id}/smth", new
            {
                smth = passFile.DataEncrypted!
            });

            return request.WithContext(passFile.GetTitle())
                .WithBadHandling(WhatToStringMapper)
                .ExecuteAsync<PassFile>();
        }

        private static Task<OkBadResponse<PassFile>?> _AddPassFileRemoteAsync(PassFile passFile)
        {
            var request = PassMetaApi.Post("/passfiles/new", new
            {
                name = passFile.Name,
                color = passFile.Color,
                created_on = passFile.CreatedOn,
                smth = passFile.DataEncrypted!
            });

            return request.WithContext(passFile.GetTitle())
                .WithBadHandling(WhatToStringMapper)
                .ExecuteAsync<PassFile>();
        }

        private static Task<OkBadResponse?> _DeletePassFileRemoteAsync(PassFile passFile, string accountPassword)
        {
            var request = PassMetaApi.Delete($"/passfiles/{passFile.Id}", new
            {
                check_password = accountPassword
            });
                
            return request.WithContext(passFile.GetTitle())
                .WithBadHandling(WhatToStringMapper)
                .ExecuteAsync();
        }

        #endregion
    }
}