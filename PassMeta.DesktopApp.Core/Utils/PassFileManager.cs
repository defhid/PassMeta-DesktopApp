using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
    
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models;

namespace PassMeta.DesktopApp.Core.Utils;

/// <summary>
/// Passfiles manager.
/// </summary>
/// <remarks>Not designed for concurrent work.</remarks>
public static class PassFileManager
{
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
    /// Save all passfile of specified type changes to the file system.
    /// </summary>
    public static async Task<IDetailedResult> CommitAsync(PassFileType ofType)
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
                if (changed.Type != ofType) continue;

                if (changed.LocalDeleted)
                {
                    delete.Add(changed);
                }
                else if (changed.VersionChangedOn != source?.VersionChangedOn 
                         || changed.DataEncrypted != source.DataEncrypted)
                {
                    if (changed.DataEncrypted is null)
                    {
                        var result = PassFileCryptoService.Encrypt(changed);
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
                var list = _currentPassFiles
                    .Select(pf => pf.source is not null 
                        ? pf.source.Type == ofType
                            ? (pf.changed ?? pf.source)
                            : pf.source
                        : pf.changed!.Type == ofType 
                            ? pf.changed 
                            : null)
                    .Where(pf => pf is not null)
                    .ToList();
 
                var result = await _SaveListAsync(list!);
                if (result.Bad) return result;

                _currentPassFiles = _currentPassFiles
                    .Select(pf => (pf.changed ?? pf.source)!.Type == ofType 
                        ? (pf.changed ?? pf.source, (PassFile?)null)
                        : pf)
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            return ManagerError("Unknown error", ex);
        }

        AnyCurrentChangedSource.OnNext(AnyCurrentChanged);

        return Result.Success(hasWarnings ? Resources.PASSCONTEXT__COMMIT_WARNING : null);
    }

    private static async Task<IDetailedResult> _SaveAsync(PassFile passFile)
    {
        try
        {
            var path = _GetUserPassFilePath(passFile.Type, passFile.Id);
            await File.WriteAllBytesAsync(path, passFile.DataEncrypted!);
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
                var tmpPath = to + "_tmp" + DateTime.UtcNow.Ticks;
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
            var path = _GetUserPassFilePath(passFile.Type, passFile.Id);
            var oldPath = _GetUserOldPassFilePath(passFile.Type, passFile.Id);
                
            if (File.Exists(path))
            {
                Move(path, oldPath);
            }
            else if (passFile.Origin is not null && passFile.Origin.Id != passFile.Id)
            {
                var originPath = _GetUserPassFilePath(passFile.Type, passFile.Origin.Id);
                var originOldPath = _GetUserOldPassFilePath(passFile.Type, passFile.Origin.Id);
                    
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
}