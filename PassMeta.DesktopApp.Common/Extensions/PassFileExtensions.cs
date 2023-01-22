using PassMeta.DesktopApp.Common.Abstractions.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="IPassFile{TContent}"/>.
/// </summary>
public static class PassFileExtensions
{
    /// <summary>
    /// Is passfile created locally, but not uploaded to the server?
    /// </summary>
    public static bool IsLocalCreated(this IPassFile passFile)
        => passFile is { Id: < 0, RemoteOrigin: null };

    /// <summary>
    /// Is passfile deleted locally, but not deleted from the server?
    /// </summary>
    public static bool IsLocalDeleted(this IPassFile passFile) 
        => passFile is not { LocalDeletedOn: null };

    /// <summary>
    /// Is passfile changed locally (created/updated/deleted), but not uploaded on the server?
    /// </summary>
    public static bool IsLocalChanged(this IPassFile passFile) 
        => passFile.IsLocalCreated() ||
           passFile.IsLocalDeleted() ||
           passFile.IsLocalInfoFieldsChanged() ||
           passFile.IsLocalVersionFieldsChanged();

    /// <summary>
    /// Is passfile information changed locally, but not uploaded on the server?
    /// </summary>
    public static bool IsLocalInfoFieldsChanged(this IPassFile passFile) 
        => passFile.RemoteOrigin is null ||
           passFile.RemoteOrigin.Id != passFile.Id ||
           passFile.RemoteOrigin.Name != passFile.Name ||
           passFile.RemoteOrigin.Color != passFile.Color;

    /// <summary>
    /// Is passfile version changed locally, but not uploaded on the server?
    /// </summary>
    public static bool IsLocalVersionFieldsChanged(this IPassFile passFile) 
        => passFile.RemoteOrigin is null ||
           passFile.RemoteOrigin.Version != passFile.Version ||
           passFile.RemoteOrigin.VersionChangedOn != passFile.VersionChangedOn;
}