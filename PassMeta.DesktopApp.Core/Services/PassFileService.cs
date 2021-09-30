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
        public async Task<PassFile?> GetPassFileRemoteAsync(int passFileId)
        {
            var response = await PassMetaApi.GetAsync<PassFile>($"/passfiles/{passFileId}", true);
            return response?.Data;
        }
        
        public async Task<Result<List<PassFile>>> GetPassFileListAsync()
        {
            var localList = await _GetPassFilesLocalAsync();

            if (AppConfig.Current.ServerVersion is null)
            {
                return new Result<List<PassFile>>(localList, Resources.INFO__PASSFILES_LOCAL_MODE);
            }
            
            var remoteList = await _GetPassFilesRemoteAsync();
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
                    resultList.Add(local);
                    continue;
                }
                
                var response = await PassMetaApi.GetAsync<PassFile>($"/passfiles/{remote.Id}", true);
                if (response?.Success is not true)
                {
                    if (local is not null) resultList.Add(local);
                    continue;
                }

                var remoteFull = response.Data;
                
                if (local is not null && local.IsChanged)
                {
                    if (local.Version == remote.Version)
                    {
                        await _SavePassFileRemoteAsync(local);
                        resultList.Add(local);
                    }
                    else
                    {
                        var result = _MergePassFiles(local, remoteFull);
                        if (result.Bad)
                            return new Result<List<PassFile>>(result.Ok, result.Message);
                        
                        if ((await _SavePassFileLocalAsync(result.Data.Item1)).Bad) continue;
                        
                        Locator.Current.GetService<IDialogService>()!.ShowInfo(
                            string.Format(Resources.INFO__PASSFILES_MERGED, remote.Name, local.Name), 
                            null, 
                            string.Join('\n', result.Data.Item2.Select(s => s.Name)));
                    }
                }
                else
                {
                    await _SavePassFileLocalAsync(remoteFull);
                }
                
                resultList.Add(remoteFull);
            }

            return new Result<List<PassFile>>(resultList);
        }

        public async Task<Result> SavePassFileAsync(PassFile passFile)
        {
            if (AppConfig.Current.ServerVersion is null)
            {
                return await _SavePassFileLocalAsync(passFile);
            }
            
            var res = await _SavePassFileRemoteAsync(passFile);
            if (res.Bad) return res;
            
            await _SavePassFileLocalAsync(passFile);
            return res;
        }
        
        public async Task<Result> ArchivePassFileAsync(PassFileLight passFile)
        {
            var res = await _ArchivePassFileRemoteAsync(passFile);
            if (res.Bad) return res;

            _DeletePassFileLocal(passFile);
            return res;
        }
        
        public async Task<Result<PassFile?>> UnArchivePassFileAsync(PassFileLight passFile)
        {
            var res = await _UnArchivePassFileRemoteAsync(passFile);
            if (res.Bad) return new Result<PassFile?>(res.Ok, res.Message);

            var pf = await GetPassFileRemoteAsync(passFile.Id);
            if (pf is not null) await _SavePassFileLocalAsync(pf);
            
            return new Result<PassFile?>(pf);
        }

        public async Task<Result> DeletePassFileAsync(PassFileLight passFile)
        {
            var res = await _DeletePassFileRemoteAsync(passFile);
            if (res.Bad) return res;
            
            _DeletePassFileLocal(passFile);
            return res;
        }
        
        private static async Task<List<PassFile>> _GetPassFilesLocalAsync()
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
                            .Confirm(Resources.ERR__INVALID_PASSFILE_FOUND + $" ({exRead})");

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
                Locator.Current.GetService<IDialogService>()!
                    .ShowError(Resources.ERR__PASSFILES_READ, more: ex.ToString());
            }

            return passFiles;
        }

        private static async Task<List<PassFileLight>?> _GetPassFilesRemoteAsync()
        {
            var response = await PassMetaApi.GetAsync<List<PassFileLight>>("/passfiles/list", true);
            return response?.Data;
        }

        private static async Task<Result> _SavePassFileLocalAsync(PassFile passFile)
        {
            try
            {
                _EnsurePassFilesDirectoryExists();
                
                passFile.Encrypt();

                await File.WriteAllTextAsync(_GetPassFilePath(passFile), JsonConvert.SerializeObject(passFile));
            }
            catch (Exception ex)
            {
                Locator.Current.GetService<IDialogService>()!
                    .ShowError(string.Format(Resources.ERR__PASSFILE_SAVE_LOCAL, passFile.Name), more: ex.ToString());

                return new Result(false);
            }

            return new Result();
        }
        
        private static async Task<Result> _SavePassFileRemoteAsync(PassFile passFile)
        {
            try
            {
                passFile.Encrypt();
                
                var postData = new PassFIlePostData
                {
                    Name = passFile.Name,
                    Color = passFile.Color,
                    Smth = passFile.DataEncrypted!
                };

                var response = await PassMetaApi.PatchAsync<PassFIlePostData, PassFileLight>($"/passfiles/{passFile.Id}", 
                    postData, true);

                if (response?.Success is not true)
                {
                    return new Result(false);
                }
                
                passFile.Refresh(response.Data);
            }
            catch (Exception ex)
            {
                Locator.Current.GetService<IDialogService>()!
                    .ShowError(string.Format(Resources.ERR__PASSFILE_SAVE_REMOTE, passFile.Name), more: ex.ToString());

                return new Result(false);
            }

            return new Result();
        }

        private static Result _DeletePassFileLocal(PassFileLight passFile)
        {
            try
            {
                _EnsurePassFilesDirectoryExists();

                var path = _GetPassFilePath(passFile);
                
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception ex)
            {
                Locator.Current.GetService<IDialogService>()!
                    .ShowError(string.Format(Resources.ERR__PASSFILE_DELETE_LOCAL, passFile.Name), more: ex.ToString());

                return new Result(false);
            }

            return new Result();
        }
        
        private static async Task<Result> _DeletePassFileRemoteAsync(PassFileLight passFile)
        {
            try
            {
                var response = await PassMetaApi.DeleteAsync<object>($"/passfiles/{passFile.Id}", true);
                return new Result(response?.Success is true);
            }
            catch (Exception ex)
            {
                Locator.Current.GetService<IDialogService>()!
                    .ShowError(string.Format(Resources.ERR__PASSFILE_DELETE_REMOTE, passFile.Name), more: ex.ToString());

                return new Result(false);
            }
        }

        private static async Task<Result> _ArchivePassFileRemoteAsync(PassFileLight passFile)
        {
            try
            {
                var response = await PassMetaApi.PutAsync<object>($"/passfiles/{passFile.Id}/to/archive", true);
                return new Result(response?.Success is true);
            }
            catch (Exception ex)
            {
                Locator.Current.GetService<IDialogService>()!
                    .ShowError(string.Format(Resources.ERR__PASSFILE_ARCHIVE_REMOTE, passFile.Name), more: ex.ToString());

                return new Result(false);
            }
        }
        
        private static async Task<Result> _UnArchivePassFileRemoteAsync(PassFileLight passFile)
        {
            try
            {
                var response = await PassMetaApi.PutAsync<object>($"/passfiles/{passFile.Id}/to/actual", true);
                return new Result(response?.Success is true);
            }
            catch (Exception ex)
            {
                Locator.Current.GetService<IDialogService>()!
                    .ShowError(string.Format(Resources.ERR__PASSFILE_UNARCHIVE_REMOTE, passFile.Name), more: ex.ToString());

                return new Result(false);
            }
        }

        private static void _EnsurePassFilesDirectoryExists()
        {
            if (!Directory.Exists(AppConfig.PassFilesPath))
                Directory.CreateDirectory(AppConfig.PassFilesPath);
        }

        private static string _GetPassFilePath(PassFileLight passFile) =>
            Path.Combine(AppConfig.PassFilesPath, passFile.Id + ".tmp");

        private static Result<(PassFile, List<PassFile.Section>)> _MergePassFiles(PassFile first, PassFile second)
        {
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
                    foreach (var item in section.Items)
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