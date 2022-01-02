namespace PassMeta.DesktopApp.Core.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Common;
    using Common.Interfaces.Services;
    using Common.Models;
    using Common.Models.Entities;
    using Extensions;
    using Newtonsoft.Json;
    using Splat;

    /// <summary>
    /// Local passfiles manager.
    /// </summary>
    /// <remarks>Not designed for concurrent work.</remarks>
    public static class PassFileLocalManager
    {
        private static ILogService Logger => Locator.Current.GetService<ILogService>()!;
        
        private const string EmptyListJson = "[]";
        
        /// <summary>
        /// Source reflects data from the local storage.
        /// Changed reflects edited uncommited data.
        /// </summary>
        private static List<(PassFile? source, PassFile? changed)> _passFiles = new();

        /// <summary>
        /// Log error and return result with common manager error message.
        /// </summary>
        private static Result ManagerError(string log, Exception? ex = null)
        {
            log = nameof(PassFileLocalManager) + ": " + log;
            if (ex is null) Logger.Error(log);
            else Logger.Error(ex, log);
            return Result.Failure(Resources.PASSMANAGER__ERR);
        }

        /// <summary>
        /// Check passfiles directory, apply autocorrection if required, load passfile list.
        /// </summary>
        /// <remarks>Errors auto-logging.</remarks>
        /// <exception cref="Exception">Throws critical exceptions.</exception>
        public static void Initialize()
        {
            if (!Directory.Exists(PassFilesPath))
            {
                Logger.Info("Passfiles directory not found, launch autocorrection...");
                _AutoCorrectPassFileDirectory(true);
            }

            if (!File.Exists(PassFileListPath))
            {
                Logger.Info("Passfile list not found, launch autocorrection...");
                _AutoCorrectPassFileList(true);
            }
            
            _LoadListAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get current passfile list.
        /// Reflects uncommitted state, changed passfiles are a priority.
        /// </summary>
        public static List<PassFile> GetCurrentList() => _passFiles
            .Select(pf => (pf.changed ?? pf.source)!.Copy())
            .ToList();
        
        /// <summary>
        /// Get source passfile list.
        /// Reflects the current state in the file system.
        /// </summary>
        public static List<PassFile> GetSourceList() => _passFiles
            .Where(pf => pf.source is not null)
            .Select(pf => pf.source!.Copy())
            .ToList();

        /// <summary>
        /// Load data for <see cref="passFile"/>.
        /// </summary>
        public static async Task<Result<string>> GetDataAsync(PassFile passFile, int? version = null)
        {
            try
            {
                string path;
                if (version is null)
                {
                    path = _GetPassFilePath(passFile.Id);
                }
                else
                {
                    if (version != passFile.Version - 1)
                    {
                        return Result.Failure<string>(string.Format(Resources.PASSMANAGER__VERSION_NOT_FOUND_ERR, version));
                    }
                    
                    path = _GetOldPassFilePath(passFile.Id);
                }
                
                return File.Exists(path)
                    ? Result.Success<string>(await File.ReadAllTextAsync(path))
                    : Result.Failure<string>(string.Format(Resources.PASSMANAGER__VERSION_NOT_FOUND_ERR, version));
            }
            catch (Exception ex)
            {
                return ManagerError("Passfile reading failed", ex).WithNullData<string>();
            }
        }

        /// <summary>
        /// Update passfile information.
        /// </summary>
        /// <param name="passFile">Passfile information.</param>
        /// <param name="fromRemote">Is <paramref name="passFile"/> from remote?</param>
        public static Result<PassFile> UpdateInfo(PassFile passFile, bool fromRemote = false)
        {
            var found = _passFiles.FindIndex(pf =>
                pf.source?.Id == passFile.Id || 
                pf.changed?.Id == passFile.Id || 
                pf.source?.Origin?.Id == passFile.Id);
            
            if (found < 0)
                return ManagerError($"Can't find {passFile} to update information!").WithNullData<PassFile>();

            var (source, changed) = _passFiles[found];

            if (source?.Name == passFile.Name && source?.Color == passFile.Color)
            {
                _passFiles[found] = (source, null);
                return Result.Success(source!.Copy());
            }

            if (changed?.Name == passFile.Name && changed?.Color == passFile.Color)
            {
                return Result.Success(changed!.Copy());
            }

            changed ??= source!.Copy(false);
            changed.Origin ??= source;
            changed.Name = passFile.Name;
            changed.Color = passFile.Color;
            changed.InfoChangedOn = DateTime.Now;

            if (fromRemote)
            {
                changed.Origin!.Name = changed.Name;
                changed.Origin!.Color = changed.Color;
                changed.Origin!.InfoChangedOn = changed.InfoChangedOn;
            }
            
            _RemoveOriginIfRequired(changed);

            _passFiles[found] = (source, changed);
            return Result.Success(changed.Copy());
        }
        
        /// <summary>
        /// Update passfile data.
        /// </summary>
        /// <param name="passFile">
        /// Passfile with <see cref="PassFile.DataEncrypted"/> or <see cref="PassFile.Data"/> and <see cref="PassFile.PassPhrase"/>.
        /// </param>
        /// <param name="fromRemote">
        /// Is <paramref name="passFile"/> from remote?
        /// </param>
        public static Result<PassFile> UpdateData(PassFile passFile, bool fromRemote = false)
        {
            var found = _passFiles.FindIndex(pf =>
                pf.source?.Id == passFile.Id || 
                pf.changed?.Id == passFile.Id || 
                pf.source?.Origin?.Id == passFile.Id);
            
            if (found < 0)
                return ManagerError($"Can't find {passFile} to update data!").WithNullData<PassFile>();

            if (passFile.Data is null && passFile.DataEncrypted is null)
                return ManagerError($"Can't update {passFile} data to null!").WithNullData<PassFile>();

            if (passFile.PassPhrase is null && passFile.DataEncrypted is null)
                return ManagerError($"Can't update {passFile} data without passphrase!").WithNullData<PassFile>();

            var (source, changed) = _passFiles[found];

            changed ??= source!.Copy(false);
            changed.Origin ??= source;
            changed.DataEncrypted = passFile.DataEncrypted;
            changed.Data = passFile.Data;
            changed.PassPhrase = passFile.PassPhrase;
            
            if (fromRemote)
            {
                changed.Version = passFile.Version;
                changed.VersionChangedOn = passFile.VersionChangedOn;
                changed.Origin!.Version = changed.Version;
                changed.Origin!.VersionChangedOn = changed.VersionChangedOn;
            }
            else
            {
                changed.Version += 1;
                changed.VersionChangedOn = DateTime.Now;
            }
            
            _RemoveOriginIfRequired(changed);

            _passFiles[found] = (source, changed);
            return Result.Success(changed.Copy());
        }

        /// <summary>
        /// Create new <see cref="PassFile"/>, set its local <see cref="PassFile.Id"/>, add to the current list.
        /// </summary>
        /// <returns>Created passfile.</returns>
        public static PassFile CreateNew()
        {
            var ids = _passFiles.Where(pf => pf.source is not null).Select(pf => pf.source!.Id).ToList();
            
            var passFile = new PassFile
            {
                Id = (ids.Any() ? Math.Min(ids.Min() - 1, 0) : 0) - 1
            };
            
            _passFiles.Add((null, passFile));
            return passFile;
        }

        /// <summary>
        /// Delete passfile from list if it hasn't source, otherwise mark changed <see cref="PassFile.Id"/> as 0.
        /// </summary>
        public static void Delete(PassFile passFile)
        {
            var found = _passFiles.FindIndex(pf => pf.source?.Id == passFile.Id);
            if (found < 0)
            {
                _passFiles.RemoveAll(pf => pf.changed?.Id == passFile.Id);
            }
            else
            {
                var (source, changed) = _passFiles[found];
            
                changed ??= source!.Copy();
                changed.Id = 0;

                _passFiles[found] = (source, changed);
            }
        }

        /// <summary>
        /// Add a new existing <see cref="PassFile"/> that is not in the current list.
        /// </summary>
        /// <remarks>No commits required.</remarks>
        public static async Task<Result> AddFinalAsync(PassFile passFile)
        {
            if (_passFiles.Any(pf => pf.source?.Id == passFile.Id || pf.changed?.Id == passFile.Id))
            {
                return ManagerError($"Adding {passFile} failed: already exists");
            }
            
            var result = await _SaveAsync(passFile);
            if (result.Ok)
            {
                _passFiles.Add((passFile.Copy(), null));

                var list = _passFiles.Where(pf => pf.source is not null)
                    .Select(pf => pf.source!).ToList();
                
                result = await _SaveListAsync(list);
            }
            
            return result;
        }

        /// <summary>
        /// Delete passfile from list finally.
        /// </summary>
        /// <remarks>No commits required.</remarks>
        public static async Task<Result> DeleteFinalAsync(PassFile passFile)
        {
            var found = _passFiles.FindIndex(pf => 
                pf.source?.Id == passFile.Id || pf.source?.Origin?.Id == passFile.Id);
            
            if (found < 0)
                return ManagerError($"Final deleting {passFile} failed: not found");

            var list = _passFiles.Where(pf => pf.source is not null && pf.source.Origin?.Id != passFile.Id)
                .Select(pf => pf.source!).ToList();

            var result = await _SaveListAsync(list);
            if (result.Ok)
                _passFiles.RemoveAt(found);

            return result;
        }

        /// <summary>
        /// Cancel all passfile changes.
        /// </summary>
        public static void Rollback()
        {
            for (var i = 0; i < _passFiles.Count; ++i)
            {
                if (_passFiles[i].changed is not null)
                {
                    _passFiles[i] = (_passFiles[i].source, null);
                }
            }
        }

        /// <summary>
        /// Save all passfile changes to the file system.
        /// </summary>
        public static async Task<Result> CommitAsync()
        {
            var infoChange = new List<PassFile>();
            var dataChange = new List<PassFile>();
            var delete = new List<PassFile>();

            var hasWarnings = false;
            
            try
            {
                #region Prepare

                foreach (var (_, changed) in _passFiles)
                {
                    if (changed is null) continue;
                    
                    if (changed.LocalDeleted && changed.Origin!.LocalCreated is true)
                    {
                        delete.Add(changed);
                        continue;
                    }
                    
                    if (changed.VersionChangedOn != changed.Origin!.VersionChangedOn)
                    {
                        if (changed.Data is not null && changed.PassPhrase is not null)
                        {
                            var result = changed.Encrypt();
                            if (result.Bad) return result;
                        }
                        dataChange.Add(changed);
                    }

                    if (changed.InfoChangedOn != changed.Origin!.InfoChangedOn)
                    {
                        infoChange.Add(changed);
                    }
                }

                #endregion

                #region Delete

                foreach (var passFile in delete)
                {
                    var ok = _Delete(passFile).Ok;
                    if (ok)
                    {
                        var index = _passFiles.FindIndex(pf => ReferenceEquals(pf.changed, passFile));
                        _passFiles.RemoveAt(index);
                    }
                    hasWarnings |= ok;
                }

                #endregion

                #region Data

                foreach (var passFile in dataChange)
                {
                    var ok = _Delete(passFile).Ok && (await _SaveAsync(passFile)).Ok;
                    if (ok)
                    {
                        if (passFile.InfoChangedOn == passFile.Origin!.InfoChangedOn)
                        {
                            var index = _passFiles.FindIndex(pf => ReferenceEquals(pf.changed, passFile));
                            _passFiles[index] = (source: passFile, null);
                        }
                    }
                    hasWarnings |= ok;
                }

                #endregion

                #region Info

                if (delete.Any() || dataChange.Any() || infoChange.Any())
                {
                    var result = await _SaveListAsync(_passFiles.Select(pf => (pf.changed ?? pf.source)!).ToList());
                    if (result.Bad) return result;
                    
                    foreach (var passFile in infoChange)
                    {
                        var index = _passFiles.FindIndex(pf => ReferenceEquals(pf.changed, passFile));
                        _passFiles[index] = (passFile, null);
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                return ManagerError("Unknown error", ex);
            }

            return Result.Success(hasWarnings ? Resources.PASSMANAGER__COMMIT_WARNING : null);
        }

        private static void _RemoveOriginIfRequired(PassFile passFile)
        {
            var origin = passFile.Origin;
            if (origin is null) return;

            if (origin.Name != passFile.Name) return;
            if (origin.Color != passFile.Color) return;
            if (origin.Version != passFile.Version) return;

            passFile.Origin = null;
        }
        
        private static async Task<Result> _SaveAsync(PassFile passFile)
        {
            try
            {
                var path = _GetPassFilePath(passFile.Id);
                await File.WriteAllTextAsync(path, passFile.DataEncrypted, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return ManagerError($"Writing {passFile} failed", ex);
            }
            
            return Result.Success();
        }

        private static Result _Delete(PassFile passFile)
        {
            try
            {
                var path = _GetPassFilePath(passFile.Id);
                var oldPath = _GetOldPassFilePath(passFile.Id);

                if (File.Exists(path))
                {
                    if (File.Exists(oldPath))
                    {
                        var tmpPath = oldPath + "_tmp" + DateTime.Now.Ticks;
                        File.Move(oldPath, tmpPath);
                        File.Move(path, oldPath);
                        File.Delete(tmpPath);
                    }
                    else
                    {
                        File.Move(path, oldPath);
                    }
                }
            }
            catch (Exception ex)
            {
                return ManagerError($"Deleting {passFile} failed", ex);
            }
            
            return Result.Success();
        }

        private static async Task<Result> _SaveListAsync(List<PassFile> list)
        {
            try
            {
                var listData = JsonConvert.SerializeObject(list);
                await File.WriteAllTextAsync(PassFileListPath, listData, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return ManagerError("Passfile list writing error", ex);
            }
            
            return Result.Success();
        }

        private static async Task _LoadListAsync()
        {
            string listData;
            try
            {
                listData = await File.ReadAllTextAsync(PassFileListPath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Passfile list reading error, new empty list will be created and used");
                _AutoCorrectPassFileList(false);
                listData = EmptyListJson;
            }

            List<PassFile> passFiles;
            try
            {
                passFiles = JsonConvert.DeserializeObject<List<PassFile>>(listData) ?? new List<PassFile>();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Passfile list deserializing error, new empty list will be created and used");
                _AutoCorrectPassFileList(false);
                passFiles = new List<PassFile>();
            }

            _passFiles = passFiles.Select(pf => (pf, (PassFile?)null)).ToList()!;
        }

        #region AutoCorrection

        private static void _AutoCorrectPassFileDirectory(bool throwIfException)
        {
            try
            {
                Directory.CreateDirectory(PassFilesPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Passfiles directory autocorrection failed");
                
                if (throwIfException) throw;
                return;
            }
            
            Logger.Info("Passfiles directory autocorrection succeed");
        }

        private static void _AutoCorrectPassFileList(bool throwIfException)
        {
            try
            {
                if (File.Exists(PassFileListPath))
                {
                    File.Move(PassFileListPath, $"{PassFileListPath}invalid{DateTime.Now:dd_MM_yyyy__mm_ss}");
                }
                
                File.WriteAllText(PassFileListPath, EmptyListJson, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Passfile list autocorrection failed");
                
                if (throwIfException) throw;
                return;
            }
            
            Logger.Info("Passfile list autocorrection succeed");
        }

        #endregion
        
        #region Paths
        
        private static readonly string PassFilesPath = AppConfig.PassFilesPath;

        private static readonly string PassFileListPath = Path.Combine(PassFilesPath, "__all__");

        private static string _GetPassFilePath(int passFileId)
            => Path.Combine(PassFilesPath, passFileId + ".passfile");

        private static string _GetOldPassFilePath(int passFileId)
            => Path.Combine(PassFilesPath, passFileId + ".passfile.old");
        
        #endregion
    }
}