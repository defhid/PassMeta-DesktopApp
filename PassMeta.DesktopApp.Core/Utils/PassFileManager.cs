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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;
    using Common.Interfaces;
    using Common.Utils.Extensions;
    using Newtonsoft.Json;

    /// <summary>
    /// Passfiles manager.
    /// </summary>
    /// <remarks>Not designed for concurrent work.</remarks>
    public static class PassFileManager
    {
        private static ILogService Logger => EnvironmentContainer.Resolve<ILogService>();
        
        private const string EmptyListJson = "[]";

        /// <summary>
        /// Represents <see cref="AnyCurrentChanged"/>.
        /// </summary>
        public static readonly BehaviorSubject<bool> AnyCurrentChangedSource = new(false);

        /// <summary>
        /// Represents <see cref="AnyChanged"/>.
        /// </summary>
        public static readonly BehaviorSubject<bool> AnyChangedSource = new(false);
        
        /// <summary>
        /// Has any uncommited changed/deleted passfile?
        /// </summary>
        public static bool AnyCurrentChanged =>
            _deletedPassFiles.Any() || 
            _currentPassFiles.Any(pf => pf.changed is not null && 
                                        (pf.source is null || 
                                         pf.changed.IsInformationDifferentFrom(pf.source) || 
                                         pf.changed.IsVersionDifferentFrom(pf.source)));
        
        /// <summary>
        /// Has any commited changed/deleted passfile?
        /// </summary>
        public static bool AnyChanged =>
            _currentPassFiles.Any(pf => pf.source?.Origin is not null);

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
        private static IDetailedResult ManagerError(string log, Exception? ex = null)
        {
            log = nameof(PassFileManager) + ": " + log;
            if (ex is null) Logger.Error(log);
            else Logger.Error(ex, log);
            return Result.Failure(Resources.PASSMGR__ERR);
        }

        /// <summary>
        /// Check passfiles directory, apply autocorrection if required, load passfile list.
        /// </summary>
        /// <remarks>Errors auto-logging.</remarks>
        /// <exception cref="Exception">Throws critical exceptions.</exception>
        public static async Task InitializeAsync()
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
            
            await _LoadListAsync();

            AnyChangedSource.OnNext(AnyChanged);
        }

        /// <summary>
        /// Get current passfile list filtered by current user id.
        /// Reflects uncommitted state, changed passfiles are a priority.
        /// </summary>
        public static List<PassFile> GetCurrentList() => _currentPassFiles
            .Select(pf => (pf.changed ?? pf.source)!)
            .Where(pf => pf.UserId == AppContext.Current.UserId)
            .Select(pf => pf.Copy())
            .ToList();

        /// <summary>
        /// Load <see cref="PassFile.DataEncrypted"/> for passfile with id = <paramref name="passFileId"/>.
        /// </summary>
        public static async Task<IDetailedResult<string>> GetEncryptedDataAsync(int passFileId, bool oldVersion = false)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFileId || 
                pf.changed?.Id == passFileId);
            
            var actual = found < 0 ? null : (_currentPassFiles[found].changed ?? _currentPassFiles[found].source)!;
            
            if (!oldVersion && actual is not null)
            {
                if (actual.Data != null)
                {
                    var res = actual.Encrypt();
                    Debug.Assert(res.Ok);
                    return res.Ok 
                        ? Result.Success(actual.DataEncrypted!) 
                        : res.WithNullData<string>();
                }
            }
            
            try
            {
                var path = oldVersion
                    ? _GetOldPassFilePath(passFileId) 
                    : _GetPassFilePath(passFileId);

                if (!File.Exists(path))
                    return Result.Failure<string>(Resources.PASSMGR__VERSION_NOT_FOUND_ERR);
                
                var dataEncrypted = PassFileConvention.Convert.EncryptedBytesToString(await File.ReadAllBytesAsync(path));
                if (!oldVersion && actual is not null)
                {
                    actual.DataEncrypted = dataEncrypted;
                }

                return Result.Success(dataEncrypted);
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
        public static async Task<IDetailedResult<List<PassFile.Section>>> TryLoadIfRequiredAndDecryptAsync(int passFileId)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFileId || 
                pf.changed?.Id == passFileId);
            
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
            var ids = _currentPassFiles.Select(pf => Math.Min(pf.source?.Id ?? 0, pf.changed?.Id ?? 0)).ToList();
            int passFileId;
            do
            {
                passFileId = -(int)++AppContext.Current.PassFilesCounter;
            } while (ids.Contains(passFileId));
            
            var passFile = new PassFile
            {
                Id = passFileId,
                Name = Resources.PASSMGR__DEFAULT_NEW_PASSFILE_NAME,
                CreatedOn = DateTime.Now,
                InfoChangedOn = DateTime.Now,
                Version = 1,
                VersionChangedOn = DateTime.Now,
                Data = new List<PassFile.Section>(),
                PassPhrase = passPhrase,
                UserId = AppContext.Current.UserId
            };
            
            _currentPassFiles.Add((null, passFile));
            
            AnyCurrentChangedSource.OnNext(AnyCurrentChanged);
            
            return passFile.Copy();
        }
        
        /// <summary>
        /// Add a new existing <see cref="PassFile"/> that is not in the current list.
        /// </summary>
        public static IDetailedResult AddFromRemote(PassFile passFile, int? localPassFileId = null)
        {
            if (_currentPassFiles.Any(pf => pf.source?.Id == passFile.Id || pf.changed?.Id == passFile.Id))
            {
                return ManagerError($"Adding {passFile} failed: already exists");
            }

            if (localPassFileId is not null)
            {
                var found = _currentPassFiles.FindIndex(pf =>
                    pf.source?.Id == localPassFileId || 
                    pf.changed?.Id == localPassFileId);

                if (found >= 0)
                {
                    var local = (_currentPassFiles[found].changed ?? _currentPassFiles[found].source)!;
                
                    passFile.DataEncrypted ??= local.DataEncrypted;
                    passFile.Origin = null;
                    _currentPassFiles.RemoveAt(found);
                }
            }
            
            if (passFile.DataEncrypted is null)
                return ManagerError($"Encrypted data not found while adding from remote {passFile}!");

            _currentPassFiles.Add((null, passFile.Copy()));
            
            AnyCurrentChangedSource.OnNext(AnyCurrentChanged);
            
            return Result.Success();
        }

        /// <summary>
        /// Set problem to passfile.
        /// </summary>
        public static bool TrySetProblem(int passFileId, PassFileProblem? problem)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFileId || 
                pf.changed?.Id == passFileId);

            if (found < 0) return false;
            
            var (source, changed) = _currentPassFiles[found];
            (changed ?? source)!.Problem = problem;
            
            return true;
        }

        /// <summary>
        /// Clear passfile problem.
        /// </summary>
        public static bool TryResetProblem(int passFileId)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFileId || 
                pf.changed?.Id == passFileId);
            
            if (found < 0) return false;
            
            var (source, changed) = _currentPassFiles[found];
            (changed ?? source)!.Problem = null;
            
            return true;
        }
        
        /// <summary>
        /// Set passphrase to passfile.
        /// </summary>
        public static bool TrySetPassPhrase(int passFileId, string? passPhrase)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFileId || 
                pf.changed?.Id == passFileId);

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
        public static IDetailedResult UpdateInfo(PassFile passFile, bool fromRemote = false)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFile.Id || 
                pf.changed?.Id == passFile.Id);
            
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
        public static IDetailedResult UpdateData(PassFile passFile, bool fromRemote = false)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFile.Id || 
                pf.changed?.Id == passFile.Id);
            
            if (found < 0)
                return ManagerError($"Can't find {passFile} to update data!");
            
            var (source, changed) = _GetPairForChange(found);

            if (fromRemote)
            {
                if (passFile.DataEncrypted is null)
                    return ManagerError($"Can't update {passFile} encrypted data to null!");
                
                changed.DataEncrypted = passFile.DataEncrypted;
                changed.Data = null;
                changed.PassPhrase = passFile.PassPhrase;
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
        public static IDetailedResult UpdateDataSelectively(PassFile passFile, Action<List<PassFile.Section>> update)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFile.Id || 
                pf.changed?.Id == passFile.Id);
            
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
                AnyCurrentChangedSource.OnNext(AnyCurrentChanged);
                return null;
            }
            
            var (source, changed) = _GetPairForChange(found);
            
            if (changed.LocalCreated || fromRemote)
            {
                _deletedPassFiles.Add(source!);
                _currentPassFiles.RemoveAt(found);
                AnyCurrentChangedSource.OnNext(AnyCurrentChanged);
                return null;
            }

            changed.LocalDeletedOn = DateTime.Now;

            var actual = _OptimizeAndSetChanged(found, source, changed);
            return actual.Copy();
        }

        /// <summary>
        /// Restore local deleted passfile.
        /// </summary>
        /// <param name="passFile">Passfile information.</param>
        public static IDetailedResult Restore(PassFile passFile)
        {
            var found = _currentPassFiles.FindIndex(pf =>
                pf.source?.Id == passFile.Id || 
                pf.changed?.Id == passFile.Id);

            if (found < 0)
                return ManagerError($"Can't find {passFile} to restore!");

            var (source, changed) = _GetPairForChange(found);
            changed.LocalDeletedOn = null;

            var actual = _OptimizeAndSetChanged(found, source, changed);
            passFile.RefreshInfoFieldsFrom(actual);

            return Result.Success();
        }

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

            _currentPassFiles.AddRange(_deletedPassFiles.Select(source => ((PassFile?)source, (PassFile?)null)));
            _deletedPassFiles.Clear();
            
            AnyCurrentChangedSource.OnNext(AnyCurrentChanged);
        }

        /// <summary>
        /// Save all passfile changes to the file system.
        /// </summary>
        public static async Task<IDetailedResult> CommitAsync()
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

                    if (changed.LocalDeleted)
                    {
                        delete.Add(changed);
                    }
                    else if (changed.VersionChangedOn != source?.VersionChangedOn)
                    {
                        if (changed.DataEncrypted is null)
                        {
                            var result = changed.Encrypt();
                            if (result.Bad) return result;
                        }
                        dataChange.Add(changed);
                    }
                    else if (changed.InfoChangedOn != source.InfoChangedOn
                             || changed.Origin is null != source.Origin is null)
                    {
                        listChange = true;
                    }
                }

                listChange |= delete.Any() || dataChange.Any();

                #endregion

                foreach (var passFile in delete)
                {
                    if (_Delete(passFile).Ok) _deletedPassFiles.Remove(passFile);
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

            AnyCurrentChangedSource.OnNext(AnyCurrentChanged);
            AnyChangedSource.OnNext(AnyChanged);

            return Result.Success(hasWarnings ? Resources.PASSMGR__COMMIT_WARNING : null);
        }

        private static (PassFile? source, PassFile changed) _GetPairForChange(int index)
        {
            var pair = _currentPassFiles[index];
            if (pair.source is null) return pair!;

            var changed = pair.changed ?? pair.source!.Copy();
            if (changed.Origin is null && pair.source?.LocalCreated is false)
            {
                changed.Origin = pair.source.Copy(false);
            }

            return (pair.source, changed);
        }

        private static PassFile _OptimizeAndSetChanged(int index, PassFile? source, PassFile? changed)
        {
            if (source != null && changed != null)
            {
                var infoDiff = changed.IsInformationDifferentFrom(source);
                var dataDiff = changed.IsVersionDifferentFrom(source);
                
                if (!infoDiff)
                    changed.InfoChangedOn = source.InfoChangedOn;
                
                if (!dataDiff)
                    changed.VersionChangedOn = source.VersionChangedOn;

                if (!infoDiff && !dataDiff && source.Origin is null)
                    changed = null;
            }
            
            if (changed != null)
            {
                if (!changed.IsInformationChanged() && !changed.IsVersionChanged())
                {
                    changed.Origin = null;
                }
            }
            
            _currentPassFiles[index] = (source, changed);
            
            AnyCurrentChangedSource.OnNext(AnyCurrentChanged);
            
            return (changed ?? source)!;
        }

        private static async Task<IDetailedResult> _SaveAsync(PassFile passFile)
        {
            try
            {
                var path = _GetPassFilePath(passFile.Id);
                await File.WriteAllBytesAsync(path, PassFileConvention.Convert.EncryptedStringToBytes(passFile.DataEncrypted!));
            }
            catch (Exception ex)
            {
                return ManagerError($"Writing {passFile} failed", ex);
            }
            
            return Result.Success();
        }

        private static IDetailedResult _Delete(PassFile passFile)
        {
            void Move(string from, string to)
            {
                if (File.Exists(to))
                {
                    var tmpPath = to + "_tmp" + DateTime.Now.Ticks;
                    File.Move(to, tmpPath);
                    File.Move(from, to);
                    File.Delete(tmpPath);
                }
                else
                {
                    File.Move(from, to);
                }
            }
            
            try
            {
                var path = _GetPassFilePath(passFile.Id);
                var oldPath = _GetOldPassFilePath(passFile.Id);
                
                if (File.Exists(path))
                {
                    Move(path, oldPath);
                }
                else if (passFile.Origin is not null && passFile.Origin.Id != passFile.Id)
                {
                    var originPath = _GetPassFilePath(passFile.Origin.Id);
                    var originOldPath = _GetOldPassFilePath(passFile.Origin.Id);
                    
                    if (File.Exists(originOldPath))
                    {
                        Move(originOldPath, oldPath);
                    }

                    if (File.Exists(originPath))
                    {
                        Move(originPath, oldPath);
                    }
                }
            }
            catch (Exception ex)
            {
                return ManagerError($"Deleting {passFile} failed", ex);
            }
            
            return Result.Success();
        }

        private static async Task<IDetailedResult> _SaveListAsync(List<PassFile> list)
        {
            try
            {
                var listData = JsonConvert.SerializeObject(list);
                await File.WriteAllTextAsync(PassFileListPath, listData, PassFileConvention.JsonEncoding);
            }
            catch (Exception ex)
            {
                return ManagerError("Passfile list writing error", ex);
            }
            
            await AppContext.SaveCurrentAsync();  // save local passfiles counter
            
            return Result.Success();
        }

        private static async Task _LoadListAsync()
        {
            string listData;
            try
            {
                listData = await File.ReadAllTextAsync(PassFileListPath, PassFileConvention.JsonEncoding);
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
                
                File.WriteAllText(PassFileListPath, EmptyListJson, PassFileConvention.JsonEncoding);
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
        
        /// <summary>
        /// Full path to passfiles directory.
        /// </summary>
        public static readonly string PassFilesPath = AppConfig.PassFilesDirectory;

        /// <summary>
        /// Full path to file that contains passfiles list.
        /// </summary>
        public static readonly string PassFileListPath = Path.Combine(PassFilesPath, "__all__");

        private static string _GetPassFilePath(int passFileId)
            => Path.Combine(PassFilesPath, passFileId + ".passfile");

        private static string _GetOldPassFilePath(int passFileId)
            => Path.Combine(PassFilesPath, passFileId + ".passfile.old");
        
        #endregion
    }
}