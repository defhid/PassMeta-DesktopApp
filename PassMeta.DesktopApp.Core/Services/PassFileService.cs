namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using DesktopApp.Core.Utils;
    
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Constants;
    using Common.Utils.Mapping;
    using Splat;
    using Utils.Extensions;

    /// <inheritdoc />
    public class PassFileService : IPassFileService
    {
        private static readonly ResourceMapper WhatMapper = new MapToResource[]
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
        public async Task<List<PassFile>> GetPassFileListAsync()
        {
            var localList = PassFileLocalManager.GetCurrentList();

            if (!await PassMetaApi.CheckConnectionAsync())
            {
                return localList;
            }
            
            var remoteList = await _GetPassFileListRemoteAsync();
            if (remoteList is null)
            {
                return localList;
            }
            
            var resultList = new List<PassFile>();
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
                        remote.Problem = PassFileProblem.DownloadingError.WithInfo(response?.Message);
                    }
                    else
                    {
                        remote.DataEncrypted = response.Data!;
                        
                        var result = PassFileLocalManager.AddFromRemote(remote);
                        if (result.Bad)
                            _dialogService.ShowError(result.Message!, remote.GetTitle());
                    }
                    
                    resultList.Add(remote);
                    continue;
                }
                
                if (local.LocalDeleted)
                {
                    if (local.InfoChangedOn < remote.InfoChangedOn ||
                        local.VersionChangedOn < remote.VersionChangedOn)
                    {
                        PassFileLocalManager.UpdateInfo(remote);
                        
                        var response = await _GetPassFileDataRemoteAsync(remote.Id);
                        if (response?.Data is null)
                        {
                            remote.Problem = PassFileProblem.DownloadingError.WithInfo(response.GetLocalizedMessage());
                            resultList.Add(remote);
                        }
                        else
                        {
                            remote.DataEncrypted = response.Data!;
                            
                            var result = PassFileLocalManager.UpdateData(remote);
                            if (result.Ok)
                                resultList.Add(result.Data!);
                            else
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
                                PassFileLocalManager.Delete(local);
                            }
                            else
                            {
                                local.Problem = PassFileProblem.RemoteDeletingError;
                                resultList.Add(local);
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
                            actual.Problem = PassFileProblem.UploadingError.WithInfo(response.GetLocalizedMessage());
                        }
                    }
                    else if (local.InfoChangedOn < remote.InfoChangedOn) actual = remote;
                    else actual = local;

                    var result = PassFileLocalManager.UpdateInfo(actual, true);
                    if (result.Ok)
                        actual = result.Data!;
                    else
                        _dialogService.ShowError(result.Message!, actual.GetTitle());

                    #endregion
                    
                    #region Version
                    
                    if (local.IsVersionChanged())
                    {
                        if (local.Origin!.Version != remote.Version)
                        {
                            actual.Problem = PassFileProblem.NeedsMerge;
                        }
                        else
                        {
                            var res = await PassFileLocalManager.GetDataAsync(actual);
                            if (res.Ok)
                            {
                                actual.DataEncrypted = res.Data!;
                                var push = await _SavePassFileDataRemoteAsync(actual);
                                if (push?.Success is not true)
                                {
                                    actual.Problem = PassFileProblem.UploadingError.WithInfo(push.GetLocalizedMessage());
                                }
                            }
                            else
                            {
                                actual.Problem = PassFileProblem.UploadingError.WithInfo(res.Message!);
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

                            var res = PassFileLocalManager.UpdateData(actual, true);
                            if (res.Ok)
                                actual = res.Data!;
                            else
                                _dialogService.ShowError(res.Message!, actual.GetTitle());
                        }
                        else
                        {
                            actual.Problem = PassFileProblem.DownloadingError.WithInfo(response.GetLocalizedMessage());
                        }
                    }
                    
                    #endregion
                    
                    resultList.Add(actual);
                    continue;
                }

                if (remote.InfoChangedOn != local.InfoChangedOn)
                {
                    var result = PassFileLocalManager.UpdateInfo(remote, true);
                    if (result.Bad)
                        _dialogService.ShowError(result.Message!, remote.GetTitle());
                }

                if (remote.Version != local.Version)
                {
                    var response = await _GetPassFileDataRemoteAsync(remote.Id);
                    if (response?.Success is true)
                    {
                        remote.DataEncrypted = response.Data!;

                        var res = PassFileLocalManager.UpdateData(remote, true);
                        if (res.Bad)
                            _dialogService.ShowError(res.Message!);
                    }
                    else
                    {
                        remote.Problem = PassFileProblem.DownloadingError.WithInfo(response.GetLocalizedMessage());
                    }
                }

                resultList.Add(remote);
            }
            
            foreach (var local in localList)
            {
                PassFileLocalManager.Delete(local);
            }

            var commitResult = await PassFileLocalManager.CommitAsync();
            if (commitResult.Bad)
                _dialogService.ShowError(commitResult.Message!);
            
            return resultList;
        }

        /// <inheritdoc />
        public async Task<List<PassFile>> ApplyPassFileLocalChangesAsync()
        {
            var list = PassFileLocalManager.GetCurrentList();

            if (await PassMetaApi.CheckConnectionAsync())
            {
                foreach (var passFile in list)
                {
                    if (passFile.LocalCreated)
                    {
                        var response = await _AddPassFileRemoteAsync(passFile);
                        if (response?.Success is true)
                        {
                            var res = PassFileLocalManager.AddFromRemote(response.Data!);
                            if (res.Bad)
                                _dialogService.ShowError(res.Message!, passFile.GetTitle());
                            
                            PassFileLocalManager.Delete(passFile, true);
                        }
                    }
                    else if (passFile.LocalChanged)
                    {
                        if (passFile.IsInformationChanged())
                        {
                            var response = await _SavePassFileInfoRemoteAsync(passFile);
                            if (response?.Success is true)
                            {
                                PassFileLocalManager.UpdateInfo(response.Data!, true);
                            }
                        }

                        if (passFile.IsVersionChanged())
                        {
                            if (passFile.DataEncrypted is null)
                            {
                                if (passFile.Data is not null)
                                {
                                    var res = passFile.Encrypt();
                                    if (res.Bad)
                                        _dialogService.ShowError(res.Message!, passFile.GetTitle());
                                }
                                else
                                {
                                    var res = await PassFileLocalManager.GetDataAsync(passFile);
                                    if (res.Bad)
                                        _dialogService.ShowError(res.Message!, passFile.GetTitle());

                                    passFile.DataEncrypted = res.Data;
                                }
                            }

                            if (passFile.DataEncrypted is not null)
                            {
                                var response = await _SavePassFileDataRemoteAsync(passFile);
                                if (response?.Success is true)
                                {
                                    var res = PassFileLocalManager.UpdateData(response.Data!, true);
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

            var commitResult = await PassFileLocalManager.CommitAsync();
            if (commitResult.Bad)
            {
                _dialogService.ShowError(commitResult.Message!);
            }

            return PassFileLocalManager.GetCurrentList();
        }

        #region Api Requests

        private static async Task<List<PassFile>?> _GetPassFileListRemoteAsync()
        {
            var response = await PassMetaApi.GetAsync<List<PassFile>>("/passfiles/list", true);
            return response?.Data;
        }
        
        private static Task<OkBadResponse<PassFile>?> _GetPassFileFullRemoteAsync(int passFileId)
        {
            return PassMetaApi.GetAsync<PassFile>($"/passfiles/{passFileId}", true);
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
                .WithBadHandling(WhatMapper)
                .ExecuteAsync<PassFile>();
        }
        
        private static Task<OkBadResponse<PassFile>?> _SavePassFileDataRemoteAsync(PassFile passFile)
        {
            var request = PassMetaApi.Patch($"/passfiles/{passFile.Id}/smth", new
            {
                smth = passFile.DataEncrypted!
            });

            return request.WithContext(passFile.GetTitle())
                .WithBadHandling(WhatMapper)
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
                .WithBadHandling(WhatMapper)
                .ExecuteAsync<PassFile>();
        }

        private static Task<OkBadResponse?> _DeletePassFileRemoteAsync(PassFile passFile, string accountPassword)
        {
            var request = PassMetaApi.Delete($"/passfiles/{passFile.Id}", new
            {
                check_password = accountPassword
            });
                
            return request.WithContext(passFile.GetTitle())
                .WithBadHandling(WhatMapper)
                .ExecuteAsync();
        }

        #endregion
    }
}