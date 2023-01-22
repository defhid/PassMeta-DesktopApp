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
        => passFile is { Id: < 0, Origin: null };

    /// <summary>
    /// Is passfile deleted locally, but not deleted from the server?
    /// </summary>
    public static bool IsLocalDeleted(this IPassFile passFile) 
        => passFile is { Id: < 0, Origin: not null };

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
        => passFile.Origin is null ||
           passFile.Origin.InfoChangedOn != passFile.InfoChangedOn;

    /// <summary>
    /// Is passfile version changed locally, but not uploaded on the server?
    /// </summary>
    public static bool IsLocalVersionFieldsChanged(this IPassFile passFile) 
        => passFile.Origin is null ||
           passFile.Origin.VersionChangedOn != passFile.VersionChangedOn;
}