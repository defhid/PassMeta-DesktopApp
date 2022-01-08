namespace PassMeta.DesktopApp.Core.Utils
{
    using Common;
    using Common.Interfaces.Services;
    using Common.Models;
    using Common.Models.Entities;
    using Common.Models.Entities.Extra;
    using Extensions;
    
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
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
        private static List<(PassFile? source, PassFile? changed)> _currentPassFiles = new();
        
        /// <summary>
        /// Finally deleted passfiles.
        /// </summary>
        private static List<PassFile> _deletedPassFiles = new();

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
        public static List<PassFile> GetCurrentList() => _currentPassFiles
            .Select(pf => (pf.changed ?? pf.source)!.Copy())
            .ToList();
        
        /// <summary>
        /// Get source passfile list.
        /// Reflects the current state in the file system.
        /// </summary>
        public static List<PassFile> GetSourceList() => _currentPassFiles
            .Where(pf => pf.source is not null)
            .Select(pf => pf.source!.Copy(false))
            .Concat(_deletedPassFiles.Select(pf => pf.Copy(false)))
            .ToList();

        /// <summary>
        /// Load <see cref="PassFile.DataEncrypted"/> for <paramref name="passFile"/>.
        /// </summary>
        public static async Task<Result<string>> GetEncryptedDataAsync(PassFile passFile, int? version = null)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFile.Id || 
                pf.changed?.Id == passFile.Id || 
                pf.source?.Origin?.Id == passFile.Id);

            if (found >= 0 && version is null)
            {
                var one = (_currentPassFiles[found].changed ?? _currentPassFiles[found].source)!;
                if (one.DataEncrypted != null)
                {
                    return Result.Success<string>(one.DataEncrypted);
                }
                if (one.Data != null)
                {
                    var res = one.Encrypt();
                    return res.Ok 
                        ? Result.Success<string>(one.DataEncrypted!) 
                        : res.WithNullData<string>();
                }
            }
            
            try
            {
                var path = version is null ? _GetPassFilePath(passFile.Id)
                    : version == passFile.Version - 1 ? _GetOldPassFilePath(passFile.Id) 
                    : null;

                if (!File.Exists(path))
                    return Result.Failure<string>(string.Format(Resources.PASSMANAGER__VERSION_NOT_FOUND_ERR, version));
                
                var dataEncrypted = await File.ReadAllTextAsync(path!);
                if (version is null && found >= 0)
                {
                    var one = (_currentPassFiles[found].changed ?? _currentPassFiles[found].source)!;
                    one.DataEncrypted = dataEncrypted;
                }

                return Result.Success<string>(dataEncrypted);
            }
            catch (Exception ex)
            {
                return ManagerError("Passfile reading failed", ex).WithNullData<string>();
            }
        }
        
        /// <summary>
        /// Create new <see cref="PassFile"/>, set its local <see cref="PassFile.Id"/>, add to the current list.
        /// </summary>
        /// <returns>Created passfile.</returns>
        public static PassFile CreateNew(string passPhrase)
        {
            var ids = _currentPassFiles.Where(pf => pf.source is not null).Select(pf => pf.source!.Id).ToList();
            
            var passFile = new PassFile
            {
                Id = (ids.Any() ? Math.Min(ids.Min() - 1, 0) : 0) - 1,
                Name = Resources.PASSMANAGER__DEFAULT_NEW_PASSFILE_NAME,
                InfoChangedOn = DateTime.Now,
                Version = 1,
                VersionChangedOn = DateTime.Now,
                Data = new List<PassFile.Section>(),
                PassPhrase = passPhrase
            };
            
            _currentPassFiles.Add((null, passFile));
            return passFile.Copy();
        }
        
        /// <summary>
        /// Add a new existing <see cref="PassFile"/> that is not in the current list.
        /// </summary>
        public static Result AddFromRemote(PassFile passFile)
        {
            if (_currentPassFiles.Any(pf => pf.source?.Id == passFile.Id || pf.changed?.Id == passFile.Id))
            {
                return ManagerError($"Adding {passFile} failed: already exists");
            }

            _currentPassFiles.Add((null, passFile.Copy()));
            return Result.Success();
        }

        /// <summary>
        /// Set problem to passfile.
        /// </summary>
        public static bool TrySetProblem(int passFileId, PassFileProblem? problem)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFileId || 
                pf.changed?.Id == passFileId || 
                pf.source?.Origin?.Id == passFileId);

            if (found < 0) return false;
            
            var (source, changed) = _currentPassFiles[found];
            (changed ?? source)!.Problem = problem;
            
            return true;
        }

        /// <summary>
        /// Update passfile information.
        /// </summary>
        /// <param name="passFile">Passfile information. Will be refreshed if success.</param>
        /// <param name="fromRemote">Is <paramref name="passFile"/> information from remote?</param>
        public static Result UpdateInfo(PassFile passFile, bool fromRemote = false)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFile.Id || 
                pf.changed?.Id == passFile.Id || 
                pf.source?.Origin?.Id == passFile.Id);
            
            if (found < 0)
                return ManagerError($"Can't find {passFile} to update information!");

            var (source, changed) = _currentPassFiles[found];

            if (source?.Name == passFile.Name && source.Color == passFile.Color)
            {
                _currentPassFiles[found] = (source, null);
                passFile.RefreshInfoFieldsFrom(source);
                return Result.Success();
            }

            if (changed?.Name == passFile.Name && changed.Color == passFile.Color)
            {
                passFile.RefreshInfoFieldsFrom(changed);
                return Result.Success();
            }

            changed ??= source!.Copy(false);
            changed.Origin ??= source;
            changed.Name = passFile.Name;
            changed.Color = passFile.Color;
            changed.InfoChangedOn = fromRemote ? passFile.InfoChangedOn : DateTime.Now;

            if (fromRemote)
            {
                changed.Origin!.Name = changed.Name;
                changed.Origin!.Color = changed.Color;
                changed.Origin!.InfoChangedOn = changed.InfoChangedOn;
            }
            
            _RemoveOriginIfRequired(changed);

            _currentPassFiles[found] = (source, changed);
            
            passFile.RefreshInfoFieldsFrom(changed);
            return Result.Success();
        }
        
        /// <summary>
        /// Update passfile data.
        /// </summary>
        /// <param name="passFile">
        /// Passfile with <see cref="PassFile.DataEncrypted"/> or <see cref="PassFile.Data"/> and <see cref="PassFile.PassPhrase"/>,
        /// depending on <paramref name="fromRemote"/>. Will be refreshed if success.
        /// </param>
        /// <param name="fromRemote">
        /// Is <paramref name="passFile"/> data from remote?
        /// </param>
        public static Result UpdateData(PassFile passFile, bool fromRemote = false)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFile.Id || 
                pf.changed?.Id == passFile.Id || 
                pf.source?.Origin?.Id == passFile.Id);
            
            if (found < 0)
                return ManagerError($"Can't find {passFile} to update data!");
            
            var (source, changed) = _currentPassFiles[found];

            changed ??= source!.Copy(false);
            changed.Origin ??= source;

            if (fromRemote)
            {
                if (passFile.DataEncrypted is null)
                    return ManagerError($"Can't update {passFile} encrypted data to null!");
                
                changed.DataEncrypted = passFile.DataEncrypted;
                changed.Data = null;
                changed.PassPhrase = null;
                changed.Version = passFile.Version;
                changed.VersionChangedOn = passFile.VersionChangedOn;
                changed.Origin!.Version = changed.Version;
                changed.Origin!.VersionChangedOn = changed.VersionChangedOn;
            }
            else
            {
                if (passFile.Data is null)
                    return ManagerError($"Can't update {passFile} data to null!");
                
                if (passFile.PassPhrase is null)
                    return ManagerError($"Can't update {passFile} data without passphrase!");
                
                changed.DataEncrypted = null;
                changed.Data = passFile.Data.Select(section => section.Copy()).ToList();
                changed.PassPhrase = passFile.PassPhrase;
                changed.Version = source is null ? changed.Version : source.Version + 1;
                changed.VersionChangedOn = DateTime.Now;
            }

            _RemoveOriginIfRequired(changed);

            _currentPassFiles[found] = (source, changed);
            passFile.RefreshDataFieldsFrom(changed, false);
            return Result.Success();
        }
        
        /// <summary>
        /// Update passfile data without redundant copying.
        /// </summary>
        /// <param name="passFile">
        /// Passfile with <see cref="PassFile.Data"/> and <see cref="PassFile.PassPhrase"/>.
        /// Will be refreshed if success.
        /// </param>
        /// <param name="update">
        /// Action that must reflect <paramref name="passFile"/> changes.
        /// Used when internal changed is decrypted.
        /// </param>
        public static Result UpdateDataSelectively(PassFile passFile, Action<List<PassFile.Section>> update)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFile.Id || 
                pf.changed?.Id == passFile.Id || 
                pf.source?.Origin?.Id == passFile.Id);
            
            if (found < 0)
                return ManagerError($"Can't find {passFile} to update data selectively!");
            
            var (source, changed) = _currentPassFiles[found];

            changed ??= source!.Copy();
            changed.Origin ??= source;
            
            changed.DataEncrypted = null;

            if (changed.Data is null)
            {
                changed.Data = passFile.Data!.Select(section => section.Copy()).ToList();
                changed.PassPhrase = passFile.PassPhrase;
            }
            else
            {
                try
                {
                    update(changed.Data);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Selective passfile data update failed");
                    
                    changed.Data = passFile.Data!.Select(section => section.Copy()).ToList();
                    changed.PassPhrase = passFile.PassPhrase;
                }
            }
            
            changed.Version = source is null ? changed.Version : source.Version + 1;
            changed.VersionChangedOn = DateTime.Now;
            
            _currentPassFiles[found] = (source, changed);
            
            passFile.RefreshDataFieldsFrom(changed, false);
            return Result.Success();
        }

        /// <summary>
        /// Delete passfile from list if it hasn't source or <paramref name="fromRemote"/>,
        /// otherwise mark changed <see cref="PassFile.Id"/> as 0.
        /// </summary>
        /// <param name="passFile">Passfile information.</param>
        /// <param name="fromRemote">Is <paramref name="passFile"/> deleted from remote? (delete finally)</param>
        public static PassFile? Delete(PassFile passFile, bool fromRemote = false)
        {
            var found = _currentPassFiles.FindIndex(pf => pf.source?.Id == passFile.Id);
            if (found < 0)
            {
                _currentPassFiles.RemoveAll(pf => pf.changed?.Id == passFile.Id);
                return null;
            }
            
            if (fromRemote)
            {
                _deletedPassFiles.Add(_currentPassFiles[found].source!);
                _currentPassFiles.RemoveAt(found);
                return null;
            }
            
            var (source, changed) = _currentPassFiles[found];
            
            changed ??= source!.Copy();
            changed.Id = 0;

            _currentPassFiles[found] = (source, changed);
            return changed.Copy();
        }

        /// <summary>
        /// Cancel all passfile changes.
        /// </summary>
        public static void Rollback()
        {
            for (var i = 0; i < _currentPassFiles.Count; ++i)
            {
                if (_currentPassFiles[i].changed is not null)
                {
                    _currentPassFiles[i] = (_currentPassFiles[i].source, null);
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
            var delete = _deletedPassFiles.ToList();

            var hasWarnings = false;
            
            try
            {
                #region Prepare

                foreach (var (_, changed) in _currentPassFiles)
                {
                    if (changed is null) continue;
                    
                    if (changed.LocalDeleted && changed.Origin!.LocalCreated is true)
                    {
                        delete.Add(changed);
                        continue;
                    }
                    
                    if (changed.VersionChangedOn != changed.Origin!.VersionChangedOn)
                    {
                        if (changed.DataEncrypted is null)
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
                    if (_Delete(passFile).Ok)
                    {
                        var index = _currentPassFiles.FindIndex(pf => ReferenceEquals(pf.changed, passFile));
                        _currentPassFiles[index] = (_currentPassFiles[index].changed, null);
                        _deletedPassFiles.Remove(passFile);
                    }
                    else hasWarnings = true;
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
                            var index = _currentPassFiles.FindIndex(pf => ReferenceEquals(pf.changed, passFile));
                            _currentPassFiles[index] = (passFile, null);
                        }
                    }
                    hasWarnings |= ok;
                }

                #endregion

                #region Info

                if (delete.Any() || dataChange.Any() || infoChange.Any())
                {
                    var result = await _SaveListAsync(_currentPassFiles.Select(pf => (pf.changed ?? pf.source)!).ToList());
                    if (result.Bad) return result;
                    
                    foreach (var passFile in infoChange)
                    {
                        var index = _currentPassFiles.FindIndex(pf => ReferenceEquals(pf.changed, passFile));
                        _currentPassFiles[index] = (passFile, null);
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
            if (!passFile.IsInformationChanged() && !passFile.IsVersionChanged())
            {
                passFile.Origin = null;
            }
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

            _currentPassFiles = passFiles.Select(pf => (pf, (PassFile?)null)).ToList()!;
            _deletedPassFiles = new List<PassFile>();
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
        
        private static readonly string PassFilesPath = AppConfig.PassFilesDirectory;

        private static readonly string PassFileListPath = Path.Combine(PassFilesPath, "__all__");

        private static string _GetPassFilePath(int passFileId)
            => Path.Combine(PassFilesPath, passFileId + ".passfile");

        private static string _GetOldPassFilePath(int passFileId)
            => Path.Combine(PassFilesPath, passFileId + ".passfile.old");
        
        #endregion
    }
}