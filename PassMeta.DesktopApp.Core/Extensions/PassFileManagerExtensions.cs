using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Utils;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="IPassFileLocalStorage"/>.
/// </summary>
public static class PassFileManagerExtensions
{
    /// <summary>
    /// Load <see cref="PassFile.ContentEncrypted"/> (if not loaded), decrypt it, set to actual passfile and return copy.
    /// </summary>
    /// <remarks><see cref="PassFile.PassPhrase"/> must be set.</remarks>
    public static Task<IDetailedResult> TryLoadIfRequiredAndDecryptAsync<TContent>(this IPassFileLocalStorage passFileLocalStorage,
        PassFile<TContent> passFile)
        where TContent : class
    {
        if (passFile.Content is not null)
        {
            
        }
        
        var found = _currentPassFiles.FindIndex(pf =>
            pf.source?.Id == passFile.Id || 
            pf.changed?.Id == passFile.Id);
            
        if (found < 0)
            return ManagerError($"Can't find passfile Id={passFile.Id} to decrypt data!").WithNullData<List<PwdSection>>();
            
        var (source, changed) = _currentPassFiles[found];
        var actual = (changed ?? source)!;

        if (actual.ContentEncrypted is null)
        {
            var res = await GetEncryptedDataAsync(passFile.Type, passFile.Id);
            if (res.Bad)
                return res.WithNullData<List<PwdSection>>();
        }

        var result = PassFileCryptoService.Decrypt(actual);
        if (result.Ok)
        {
            passFile.WithDecryptedContentFrom(actual);
        }

        return result;
    }
}