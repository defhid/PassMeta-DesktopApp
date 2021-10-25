using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Models.Entities.Request;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Core.Utils;
using Splat;

namespace PassMeta.DesktopApp.Core.Services
{
    public class PassFileService : IPassFileService
    {
        private static Dictionary<string, string> WhatMapper => new()
        {
            ["check_password"] = Resources.STORAGE__DICT__CHECK_PASSWORD,
            // ...
        };

        public async Task<PassFile?> GetPassFileRemoteAsync(int passFileId)
        {
            var response = await PassMetaApi.GetAsync<PassFile>($"/passfiles/{passFileId}", true);
            return response?.Data;
        }
        
        public async Task<Result<List<PassFile>>> GetPassFileListAsync()
        {
            var localList = await _GetPassFileListLocalAsync();

            if (AppConfig.Current.ServerVersion is null)
            {
                return new Result<List<PassFile>>(localList, Resources.INFO__PASSFILES_LOCAL_MODE);
            }
            
            var remoteList = await _GetPassFileListRemoteAsync();
            if (remoteList is null)
            {
                return new Result<List<PassFile>>(localList, Resources.INFO__PASSFILES_LOCAL_MODE);
            }
            
            var resultList = new List<PassFile>();
            foreach (var remote in remoteList)
            {
                var local = localList.FirstOrDefault(pf => pf.Id == remote.Id);
                
                if (local is not null && local.Version == remote.Version)
                {
                    if (local.IsChanged)  // local changed, remote not changed
                    {
                        await _SavePassFileRemoteAsync(local);
                        resultList.Add(local);
                    }
                    else  // local not changed, remote not changed
                    {
                        resultList.Add(local);
                    }
                    
                    continue;
                }

                // local not changed, remote changed or new
                if (local is null || (local.Version < remote.Version && !local.IsChanged))
                {
                    var response = await PassMetaApi.GetAsync<PassFile>($"/passfiles/{remote.Id}", true);
                    PassFile full;
                    
                    if (response?.Success is true)
                    {
                        full = response.Data!;
                        await _SavePassFileLocalAsync(full);
                        resultList.Add(full);
                    }
                    else if (local is null)
                    {
                        full = new PassFile
                        {
                            HasProblem = true
                        };
                        full.Refresh(remote);
                    }
                    else
                    {
                        full = local;
                    }
                    
                    resultList.Add(full);
                    continue;
                }

                // local changed, remote changed
                local.NeedsMergeWith = remote;
                resultList.Add(local);
            }

            return new Result<List<PassFile>>(resultList);
        }

        public async Task<Result> SavePassFileAsync(PassFile passFile)
        {
            passFile.Encrypt();
            
            if (AppConfig.Current.ServerVersion is null)
            {
                return await _SavePassFileLocalAsync(passFile);
            }
            
            var res = await _SavePassFileRemoteAsync(passFile);
            if (res.Bad) return res;
            
            await _SavePassFileLocalAsync(passFile);
            return res;
        }
        
        public async Task<Result<PassFile?>> ArchivePassFileAsync(PassFileLight passFile)
        {
            var res = await _ArchivePassFileRemoteAsync(passFile);
            if (res.Bad) return new Result<PassFile?>(false, res.Message);

            var pf = await GetPassFileRemoteAsync(passFile.Id);
            if (pf is not null) await _SavePassFileLocalAsync(pf);
            
            return new Result<PassFile?>(pf);
        }
        
        public async Task<Result<PassFile?>> UnArchivePassFileAsync(PassFileLight passFile)
        {
            var res = await _UnArchivePassFileRemoteAsync(passFile);
            if (res.Bad) return new Result<PassFile?>(res.Ok, res.Message);

            var pf = await GetPassFileRemoteAsync(passFile.Id);
            if (pf is not null) await _SavePassFileLocalAsync(pf);
            
            return new Result<PassFile?>(pf);
        }

        public async Task<Result> DeletePassFileAsync(PassFileLight passFile, string accountPassword)
        {
            var res = await _DeletePassFileRemoteAsync(passFile, accountPassword);
            if (res.Bad) return res;
            
            await _DeletePassFileLocalAsync(passFile);
            return res;
        }
        
        private static async Task<List<PassFile>> _GetPassFileListLocalAsync()
        {
            var passFiles = new List<PassFile>();
            
            try
            {
                _EnsurePassFilesDirectoryExists();
                
                foreach (var filePath in Directory.EnumerateFiles(AppConfig.PassFilesPath))
                {
                    try
                    {
                        var fileString = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                        var passFile = JsonConvert.DeserializeObject<PassFile>(fileString);
                        if (passFile is not null)
                        {
                            passFiles.Add(passFile);
                        }
                        else
                        {
                            throw new FormatException(fileString);
                        }
                    }
                    catch (Exception exRead)
                    {
                        var delete = await Locator.Current.GetService<IDialogService>()!
                            .ConfirmAsync(Resources.ERR__INVALID_PASSFILE_FOUND + $" ({exRead})");

                        if (delete.Ok)
                        {
                            try
                            {
                                File.Delete(filePath);
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Locator.Current.GetService<IDialogService>()!
                    .ShowErrorAsync(Resources.ERR__PASSFILES_READ, more: ex.ToString());
            }

            return passFiles;
        }

        private static async Task<List<PassFileLight>?> _GetPassFileListRemoteAsync()
        {
            var response = await PassMetaApi.GetAsync<List<PassFileLight>>("/passfiles/list", true);
            return response?.Data;
        }

        private static async Task<Result> _SavePassFileLocalAsync(PassFile passFile)
        {
            try
            {
                _EnsurePassFilesDirectoryExists();

                await File.WriteAllTextAsync(_GetPassFilePath(passFile), JsonConvert.SerializeObject(passFile));
            }
            catch (Exception ex)
            {
                await Locator.Current.GetService<IDialogService>()!
                    .ShowErrorAsync(string.Format(Resources.ERR__PASSFILE_SAVE_LOCAL, passFile.Name), more: ex.ToString());

                return Result.Failure;
            }

            return Result.Success;
        }
        
        private static async Task<Result> _SavePassFileRemoteAsync(PassFile passFile)
        {
            try
            {
                var postData = new PassFilePostData(passFile.Name, passFile.Color, passFile.DataEncrypted!, passFile.CheckKey);

                var request = passFile.Id > 0
                    ? PassMetaApi.Patch($"/passfiles/{passFile.Id}", postData)
                    : PassMetaApi.Post("/passfiles/new", postData);

                var response = await request.WithBadHandling(WhatMapper).ExecuteAsync<PassFileLight>();
                if (response?.Success is not true)
                {
                    return Result.Failure;
                }
                
                passFile.Refresh(response.Data!);
            }
            catch (Exception ex)
            {
                await Locator.Current.GetService<IDialogService>()!
                    .ShowErrorAsync(string.Format(Resources.ERR__PASSFILE_SAVE_REMOTE, passFile.Name), more: ex.ToString());

                return Result.Failure;
            }

            return Result.Success;
        }

        private static async Task<Result> _DeletePassFileLocalAsync(PassFileLight passFile)
        {
            try
            {
                _EnsurePassFilesDirectoryExists();
                _EnsureOldPassFilesDirectoryExists();

                var fromPath = _GetPassFilePath(passFile);
                var toPath = _GetOldPassFilePath(passFile);

                if (File.Exists(fromPath))
                    File.Move(fromPath, toPath);
            }
            catch (Exception ex)
            {
                await Locator.Current.GetService<IDialogService>()!
                    .ShowErrorAsync(string.Format(Resources.ERR__PASSFILE_DELETE_LOCAL, passFile.Name), more: ex.ToString());

                return Result.Failure;
            }

            return Result.Success;
        }
        
        private static async Task<Result> _DeletePassFileRemoteAsync(PassFileLight passFile, string accountPassword)
        {
            try
            {
                var request = PassMetaApi.Delete($"/passfiles/{passFile.Id}", new { check_password = accountPassword });
                var response = await request.WithBadHandling(WhatMapper).ExecuteAsync();
                
                return new Result(response?.Success is true);
            }
            catch (Exception ex)
            {
                await Locator.Current.GetService<IDialogService>()!
                    .ShowErrorAsync(string.Format(Resources.ERR__PASSFILE_DELETE_REMOTE, passFile.Name), more: ex.ToString());

                return Result.Failure;
            }
        }

        private static async Task<Result> _ArchivePassFileRemoteAsync(PassFileLight passFile)
        {
            try
            {
                var response = await PassMetaApi.Put($"/passfiles/{passFile.Id}/to/archive")
                    .WithBadHandling(WhatMapper).ExecuteAsync();
                
                return new Result(response?.Success is true);
            }
            catch (Exception ex)
            {
                await Locator.Current.GetService<IDialogService>()!
                    .ShowErrorAsync(string.Format(Resources.ERR__PASSFILE_ARCHIVE_REMOTE, passFile.Name), more: ex.ToString());

                return Result.Failure;
            }
        }
        
        private static async Task<Result> _UnArchivePassFileRemoteAsync(PassFileLight passFile)
        {
            try
            {
                var response = await PassMetaApi.Put($"/passfiles/{passFile.Id}/to/actual")
                    .WithBadHandling(WhatMapper).ExecuteAsync();
                
                return new Result(response?.Success is true);
            }
            catch (Exception ex)
            {
                await Locator.Current.GetService<IDialogService>()!
                    .ShowErrorAsync(string.Format(Resources.ERR__PASSFILE_UNARCHIVE_REMOTE, passFile.Name), more: ex.ToString());

                return Result.Failure;
            }
        }

        private static void _EnsurePassFilesDirectoryExists()
        {
            if (!Directory.Exists(AppConfig.PassFilesPath))
                Directory.CreateDirectory(AppConfig.PassFilesPath);
        }
        
        private static void _EnsureOldPassFilesDirectoryExists()
        {
            var path = Path.Combine(AppConfig.PassFilesPath, ".old");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static string _GetPassFilePath(PassFileLight passFile) =>
            Path.Combine(AppConfig.PassFilesPath, passFile.Id + ".tmp");
        
        private static string _GetOldPassFilePath(PassFileLight passFile) =>
            Path.Combine(AppConfig.PassFilesPath, ".old", passFile.Id + ".tmp");

        private static Result<(PassFile, List<PassFile.Section>)> _MergePassFiles(PassFile first, PassFile second)
        {
            // var result = _MergePassFiles(local, remoteFull);
            // if (result.Bad)
            //     return new Result<List<PassFile>>(result.Ok, result.Message);
            //             
            // if ((await _SavePassFileLocalAsync(result.Data.Item1)).Bad) continue;
            //             
            // await Locator.Current.GetService<IDialogService>()!.ShowInfoAsync(
            //     string.Format(Resources.INFO__PASSFILES_MERGED, remote.Name, local.Name), 
            //     null, 
            //     string.Join('\n', result.Data.Item2.Select(s => s.Name)));
            
            if (first.Id != second.Id)
                return new Result<(PassFile, List<PassFile.Section>)>(false, 
                    "Attempt to merge passfiles with different ID!");

            if (first.Data is null || second.Data is null)
                return new Result<(PassFile, List<PassFile.Section>)>(false, 
                    "Attempt to merge passfiles without decryption!");

            if (second.ChangedLocalOn.HasValue && !first.ChangedLocalOn.HasValue)
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
                        if (currentItem is null || currentItem.Value != item.Value)
                        {
                            mergeSection.Items.Add(item);
                            changed.Add(mergeSection);
                        }
                    }
                }
            }
            
            result.Data.Sort((s1, s2) => 
                string.CompareOrdinal(s1.Name.ToLower(), s2.Name.ToLower()));

            return new Result<(PassFile, List<PassFile.Section>)>((result, changed));
        }
    }
}