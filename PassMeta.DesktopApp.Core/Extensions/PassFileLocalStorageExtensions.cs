using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Utils;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="IPassFileLocalStorage"/>.
/// </summary>
public static class PassFileLocalStorageExtensions
{
    /// <summary>
    /// Load and set local encrypted content to passfile.
    /// </summary>
    public static async Task<IDetailedResult> LoadEncryptedContentAsync<TContent>(
        this IPassFileLocalStorage storage,
        PassFile<TContent> passFile,
        IUserContext userContext,
        CancellationToken cancellationToken = default)
        where TContent : class
    {
        var result = await storage.LoadEncryptedContentAsync(
            passFile.Type, passFile.Id, passFile.Version, userContext, cancellationToken);

        if (result.Ok)
        {
            passFile.Content = new PassFileContent<TContent>(result.Data!);
        }

        return result;
    }
}