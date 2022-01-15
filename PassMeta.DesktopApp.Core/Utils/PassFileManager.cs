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
    /// Application passfiles manager.
    /// </summary>
    /// <remarks>Not designed for concurrent work.</remarks>
    public static class PassFileManager
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
            log = nameof(PassFileManager) + ": " + log;
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
        /// Load <see cref="PassFile.DataEncrypted"/> for passfile with id = <paramref name="passFileId"/>.
        /// </summary>
        public static async Task<Result<string>> GetEncryptedDataAsync(int passFileId, bool oldVersion = false)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFileId || 
                pf.changed?.Id == passFileId || 
                pf.source?.Origin?.Id == passFileId);
            
            var actual = found < 0 ? null : (_currentPassFiles[found].changed ?? _currentPassFiles[found].source)!;
            
            if (!oldVersion && actual is not null)
            {
                if (actual.DataEncrypted != null)
                {
                    return Result.Success<string>(actual.DataEncrypted);
                }
                if (actual.Data != null)
                {
                    var res = actual.Encrypt();
                    return res.Ok 
                        ? Result.Success<string>(actual.DataEncrypted!) 
                        : res.WithNullData<string>();
                }
            }
            
            try
            {
                var path = oldVersion
                    ? _GetOldPassFilePath(passFileId) 
                    : _GetPassFilePath(passFileId);

                if (!File.Exists(path))
                    return Result.Failure<string>(Resources.PASSMANAGER__VERSION_NOT_FOUND_ERR);
                
                var dataEncrypted = await File.ReadAllTextAsync(path);
                if (!oldVersion && actual is not null)
                {
                    actual.DataEncrypted = dataEncrypted;
                }

                return Result.Success<string>(dataEncrypted);
            }
            catch (Exception ex)
            {
                return ManagerError("Passfile reading failed", ex).WithNullData<string>();
            }
        }

        /// <summary>
        /// Load <see cref="PassFile.DataEncrypted"/> (if not loaded), decrypt it, set to actual passfile and return copy.
        /// </summary>
        /// <remarks><see cref="PassFile.PassPhrase"/> must be set.</remarks>
        public static async Task<Result<List<PassFile.Section>>> TryLoadIfRequiredAndDecryptAsync(int passFileId)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFileId || 
                pf.changed?.Id == passFileId || 
                pf.source?.Origin?.Id == passFileId);
            
            if (found < 0)
                return ManagerError($"Can't find passfile Id={passFileId} to decrypt data!").WithNullData<List<PassFile.Section>>();
            
            var (source, changed) = _currentPassFiles[found];
            var actual = (changed ?? source)!;

            if (actual.DataEncrypted is null)
            {
                var res = await GetEncryptedDataAsync(passFileId);
                if (res.Bad)
                    return res.WithNullData<List<PassFile.Section>>();
            }

            var result = actual.Decrypt();
            return result.Ok 
                ? Result.Success(actual.Data!.Select(section => section.Copy()).ToList()) 
                : result.WithNullData<List<PassFile.Section>>();
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
                CreatedOn = DateTime.Now,
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
        /// Set passphrase to passfile.
        /// </summary>
        public static bool TrySetPassPhrase(int passFileId, string? passPhrase)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFileId || 
                pf.changed?.Id == passFileId || 
                pf.source?.Origin?.Id == passFileId);

            if (found < 0) return false;
            
            var (source, changed) = _currentPassFiles[found];
            (changed ?? source)!.PassPhrase = passPhrase;
            
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

            var (source, changed) = _GetPairForChange(found);

            changed.Name = passFile.Name;
            changed.Color = passFile.Color;
            changed.InfoChangedOn = fromRemote ? passFile.InfoChangedOn : DateTime.Now;

            if (fromRemote)
            {
                changed.Origin!.Name = changed.Name;
                changed.Origin!.Color = changed.Color;
                changed.Origin!.InfoChangedOn = changed.InfoChangedOn;
            }

            var actual = _OptimizeAndSetChanged(found, source, changed);
            
            passFile.RefreshInfoFieldsFrom(actual);
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
            
            var (source, changed) = _GetPairForChange(found);

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

            var actual = _OptimizeAndSetChanged(found, source, changed);
            
            passFile.RefreshDataFieldsFrom(actual, false);
            return Result.Success();
        }
        
        /// <summary>
        /// Update passfile data without redundant copying.
        /// </summary>
        /// <param name="passFile">
        /// Passfile with <see cref="PassFile.Data"/> and <see cref="PassFile.PassPhrase"/>.
        /// Will be refreshed (including data) if success.
        /// </param>
        /// <param name="update">
        /// Action to perform on <paramref name="passFile"/> data.
        /// </param>
        /// <remarks>Ensure that <paramref name="update"/> algorithm only works with copies of new data!</remarks>
        public static Result UpdateDataSelectively(PassFile passFile, Action<List<PassFile.Section>> update)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFile.Id || 
                pf.changed?.Id == passFile.Id || 
                pf.source?.Origin?.Id == passFile.Id);
            
            if (found < 0)
                return ManagerError($"Can't find {passFile} to update data selectively!");
            
            var (source, changed) = _GetPairForChange(found);
            
            changed.DataEncrypted = null;

            if (changed.Data is null)
            {
                changed.Data = passFile.Data!.Select(section => section.Copy()).ToList();
                changed.PassPhrase = passFile.PassPhrase;
            }
            
            try
            {
                update(changed.Data);
                update(passFile.Data!);
            }
            catch (Exception ex)
            {
                return ManagerError("Selective passfile data update failed", ex);
            }
            
            changed.Version = source is null ? changed.Version : source.Version + 1;
            changed.VersionChangedOn = DateTime.Now;

            var actual = _OptimizeAndSetChanged(found, source, changed);
            
            passFile.RefreshDataFieldsFrom(actual, false);
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
            
            var (source, changed) = _GetPairForChange(found);
            
            if (changed.LocalCreated || fromRemote)
            {
                _deletedPassFiles.Add(changed);
                _currentPassFiles.RemoveAt(found);
                return null;
            }

            changed.Id = 0;
            changed.InfoChangedOn = DateTime.Now;

            var actual = _OptimizeAndSetChanged(found, source, changed);
            return actual.Copy();
        }

        /// <summary>
        /// Has any uncommited changed/deleted passfile?
        /// </summary>
        public static bool AnyCurrentChanged =>
            _deletedPassFiles.Any() || _currentPassFiles.Any(pf => pf.changed is not null);

        /// <summary>
        /// Cancel all passfile changes without finally deleted.
        /// </summary>
        public static void Rollback()
        {
            for (var i = _currentPassFiles.Count - 1; i >= 0; --i)
            {
                if (_currentPassFiles[i].changed is not null)
                {
                    if (_currentPassFiles[i].source is null)
                    {
                        _currentPassFiles.RemoveAt(i);
                    }
                    else
                    {
                        _currentPassFiles[i] = (_currentPassFiles[i].source, null);
                    }
                }
            }
        }

        /// <summary>
        /// Save all passfile changes to the file system.
        /// </summary>
        public static async Task<Result> CommitAsync()
        {
            var hasWarnings = false;
            var listChange = false;
            var dataChange = new List<PassFile>();
            var delete = _deletedPassFiles.ToList();
            
            try
            {
                #region Prepare

                foreach (var (source, changed) in _currentPassFiles)
                {
                    if (changed is null) continue;

                    if (changed.LocalDeleted && source?.LocalCreated is true)
                    {
                        delete.Add(changed);
                        continue;
                    }
                    
                    if (changed.VersionChangedOn != source?.VersionChangedOn)
                    {
                        if (changed.DataEncrypted is null)
                        {
                            var result = changed.Encrypt();
                            if (result.Bad) return result;
                        }
                        dataChange.Add(changed);
                        listChange = true;
                    }
                    else if (changed.InfoChangedOn != source.InfoChangedOn)
                    {
                        listChange = true;
                    }
                }

                listChange |= delete.Any();

                #endregion

                foreach (var passFile in delete)
                {
                    if (_Delete(passFile).Ok)
                    {
                        if (!_deletedPassFiles.Remove(passFile))
                        {
                            var index = _currentPassFiles.FindIndex(pf => ReferenceEquals(pf.changed, passFile));
                            _currentPassFiles[index] = (_currentPassFiles[index].changed, null);
                        }
                    }
                    else hasWarnings = true;
                }

                foreach (var passFile in dataChange)
                {
                    var ok = _Delete(passFile).Ok && (await _SaveAsync(passFile)).Ok;
                    hasWarnings |= !ok;
                }

                if (listChange)
                {
                    var list = _currentPassFiles.Select(pf => (pf.changed ?? pf.source)!).ToList();
                    var result = await _SaveListAsync(list);
                    if (result.Bad) return result;

                    _currentPassFiles = list.Select(source => ((PassFile?)source, (PassFile?)null)).ToList();
                }
            }
            catch (Exception ex)
            {
                return ManagerError("Unknown error", ex);
            }

            return Result.Success(hasWarnings ? Resources.PASSMANAGER__COMMIT_WARNING : null);
        }

        private static (PassFile? source, PassFile changed) _GetPairForChange(int index)
        {
            var pair = _currentPassFiles[index];
            
            var changed = pair.changed ?? pair.source!.Copy();
            if (changed.Origin is null && pair.source?.LocalCreated is false)
            {
                changed.Origin = pair.source;
            }

            return (pair.source, changed);
        }

        private static PassFile _OptimizeAndSetChanged(int index, PassFile? source, PassFile? changed)
        {
            if (source != null && changed != null)
            {
                {
                    var infoDiff = changed.IsInformationDifferentFrom(source);
                    var dataDiff = changed.IsVersionDifferentFrom(source);
                
                    if (!infoDiff)
                        changed.InfoChangedOn = source.InfoChangedOn;
                
                    if (!dataDiff)
                        changed.VersionChangedOn = source.VersionChangedOn;

                    if (!infoDiff && !dataDiff)
                        changed = null;
                }
                if (changed != null)
                {
                    var currChanged = _currentPassFiles[index].changed;
                    if (currChanged != null)
                    {
                        var infoDiff = changed.IsInformationDifferentFrom(currChanged);
                        var dataDiff = changed.IsVersionDifferentFrom(currChanged);
                
                        if (!infoDiff)
                            changed.InfoChangedOn = currChanged.InfoChangedOn;
                
                        if (!dataDiff)
                            changed.VersionChangedOn = currChanged.VersionChangedOn;

                        if (!infoDiff && !dataDiff)
                            changed = currChanged;
                    }
                    
                    if (!changed.IsInformationChanged() && !changed.IsVersionChanged())
                    {
                        changed.Origin = null;
                    }
                }
            }
            
            _currentPassFiles[index] = (source, changed);
            return (changed ?? source)!;
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

        private static Result _Delete(PassFile passFile)  // TODO: for storage manager
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