namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using DesktopApp.Core.Utils;
    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Constants;
    using Splat;
    using Utils.Extensions;

    /// <inheritdoc />
    public class PassFileService : IPassFileService
    {
        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;
        
        private static Dictionary<string, string> WhatMapper => new()
        {
            ["check_password"] = Resources.STORAGE__DICT__CHECK_PASSWORD,
            // TODO: ...
        };

        /// <inheritdoc />
        public async Task<PassFile?> GetPassFileWithDataRemoteAsync(int passFileId) 
            => (await _GetPassFileFullRemoteAsync(passFileId))?.Data;

        /// <inheritdoc />
        public async Task<List<PassFile>> GetPassFileListAsync()
        {
            var localList = PassFileLocalManager.GetCurrentList();

            if (AppConfig.Current.ServerVersion is null)
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
                        
                        var result = await PassFileLocalManager.AddFinalAsync(remote);
                        if (result.Bad && result.Message is not null)
                        {
                            _dialogService.ShowError(result.Message);
                        }
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
                            {
                                resultList.Add(result.Data!);
                            }
                            else
                            {
                                _dialogService.ShowError(result.Message!);
                            }
                        }
                    }
                    else
                    {
                        var answer = await _dialogService.AskPasswordAsync(
                            string.Format(Resources.PASSERVICE__ASK_PASSWORD_TO_DELETE_PASSFILE, remote.Name, local.InfoChangedOn.ToShortDateString()));
                        
                        if (answer.Ok)
                        {
                            var response = await _DeletePassFileRemoteAsync(remote, answer.Data!);
                            if (response?.Success is true)
                            {
                                var result = await PassFileLocalManager.DeleteFinalAsync(local);
                                if (result.Bad)
                                {
                                    _dialogService.ShowError(result.Message!);
                                }
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
                    {
                        actual = result.Data!;
                    }
                    else
                    {
                        _dialogService.ShowError(result.Message!);
                    }
                    
                    #endregion
                    
                    #region Version
                    
                    if (local.Changed(pf => pf.Version))
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
                                _dialogService.ShowError(res.Message!);
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
                            {
                                actual = res.Data!;
                            }
                            else
                            {
                                _dialogService.ShowError(res.Message!);
                            }
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
                    {
                        _dialogService.ShowError(result.Message!);
                    }
                }

                if (remote.Version != local.Version)
                {
                    var response = await _GetPassFileDataRemoteAsync(remote.Id);
                    if (response?.Success is true)
                    {
                        remote.DataEncrypted = response.Data!;

                        var res = PassFileLocalManager.UpdateData(remote, true);
                        if (res.Bad)
                        {
                            _dialogService.ShowError(res.Message!);
                        }
                    }
                    else
                    {
                        remote.Problem = PassFileProblem.DownloadingError.WithInfo(response.GetLocalizedMessage());
                    }
                }

                resultList.Add(remote);
            }

            var commitResult = await PassFileLocalManager.CommitAsync();
            if (commitResult.Bad)
            {
                _dialogService.ShowError(commitResult.Message!);
            }

            return resultList;
        }

        /// <inheritdoc />
        public Task<PassFile> SavePassFileInfoAsync(PassFile passFile)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<PassFile> SavePassFileDataAsync(PassFile passFile)
        {
            // TODO: complex! Unite (update info, update data, delete and create) into 'ApplyPassFileChangesAsync'.

            var encryptionResult = passFile.Encrypt();
            if (encryptionResult.Bad)
            {
                _dialogService.ShowError(encryptionResult.Message!);
                return passFile;
            }

            var remoteOk = false;

            if (AppConfig.Current.ServerVersion is not null)
            {
                var response = await _SavePassFileDataRemoteAsync(passFile);
                remoteOk = response?.Success is true;
            }

            var updateResult = PassFileLocalManager.UpdateData(passFile, remoteOk);
            if (updateResult.Bad)
            {
                var result = await PassFileLocalManager.CommitAsync();
                if (result.Ok)
                {
                    return updateResult.Data!;
                }
                
                _dialogService.ShowError(result.Message!);
                return passFile;
            }
            
            _dialogService.ShowError(updateResult.Message!);
            return passFile;
        }

        /// <inheritdoc />
        public async Task<Result> DeletePassFileAsync(PassFile passFile, string accountPassword)
        {
            var remoteOk = false;

            if (AppConfig.Current.ServerVersion is not null)
            {
                var response = await _SavePassFileDataRemoteAsync(passFile);
                remoteOk = response?.Success is true;
            }
            
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

            return request.WithBadHandling().ExecuteAsync<PassFile>();
        }
        
        private static Task<OkBadResponse<PassFile>?> _SavePassFileDataRemoteAsync(PassFile passFile)
        {
            var request = PassMetaApi.Patch($"/passfiles/{passFile.Id}/smth", new
            {
                smth = passFile.DataEncrypted!
            });

            return request.WithBadHandling(WhatMapper).ExecuteAsync<PassFile>();
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

            return request.WithBadHandling(WhatMapper).ExecuteAsync<PassFile>();
        }

        private static Task<OkBadResponse?> _DeletePassFileRemoteAsync(PassFile passFile, string accountPassword)
        {
            var request = PassMetaApi.Delete($"/passfiles/{passFile.Id}", new
            {
                check_password = accountPassword
            });
                
            return request.WithBadHandling(WhatMapper).ExecuteAsync();
        }

        #endregion

        private static Result<(PassFile, List<PassFile.Section>)> _MergePassFiles(PassFile first, PassFile second)
        {
            // var result = _MergePassFiles(local, remoteFull);
            // if (result.Bad)
            //     return new Result<List<PassFile>>(result.Ok, result.Message);
            //             
            // if ((await _SavePassFileLocalAsync(result.Data.Item1)).Bad) continue;
            //             
            // _dialogService.ShowInfoAsync(
            //     string.Format(Resources.INFO__PASSFILES_MERGED, remote.Name, local.Name), 
            //     null, 
            //     string.Join('\n', result.Data.Item2.Select(s => s.Name)));
            
            if (first.Id != second.Id)
                return Result.Failure<(PassFile, List<PassFile.Section>)>("Attempt to merge passfiles with different ID!");

            if (first.Data is null || second.Data is null)
                return Result.Failure<(PassFile, List<PassFile.Section>)>("Attempt to merge passfiles without decryption!");

            if (second.LocalChangedOn.HasValue && !first.LocalChangedOn.HasValue)
            {
                (first, second) = (second, first);
            }

            var result = new PassFile
            {
                Id = second.Id,
                CreatedOn = second.ChangedOn,
                ChangedOn = second.ChangedOn,
                Version = second.Version,
                Color = second.Color,
                Data = first.Data.ToList(),
            };

            var changed = new HashSet<PassFile.Section>();

            foreach (var section in second.Data!)
            {
                var currentSection = result.Data.FirstOrDefault(s => s.Name == second.Name);
                if (currentSection is null)
                {
                    var newSection = section.Copy();
                    result.Data.Add(newSection);
                    changed.Add(newSection);
                }
                else
                {
                    var mergeSection = currentSection.Copy();
                    foreach (var item in section.Items)  // TODO null handling
                    {
                        var currentItem = mergeSection.Items.FirstOrDefault(i => i.What == item.What);
                        if (currentItem is null || currentItem.Password != item.Password)
                        {
                            mergeSection.Items.Add(item);
                            changed.Add(mergeSection);
                        }
                    }
                }
            }
            
            result.Data.Sort((s1, s2) => 
                string.CompareOrdinal(s1.Name.ToLower(), s2.Name.ToLower()));

            return Result.Success((result, changed.ToList()));
        }
    }
}