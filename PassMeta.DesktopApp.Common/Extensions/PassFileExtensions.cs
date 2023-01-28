using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="PassFile"/>.
/// </summary>
public static class PassFileExtensions
{
    /// <summary>
    /// Is passfile created locally, but not uploaded to the server?
    /// </summary>
    public static bool IsLocalCreated(this PassFile passFile)
        => passFile is { Id: < 0 };

    /// <summary>
    /// Is passfile deleted locally, but not deleted from the server?
    /// </summary>
    public static bool IsLocalDeleted(this PassFile passFile) 
        => passFile is { DeletedOn: not null };

    /// <summary>
    /// Is passfile changed locally (created/updated/deleted), but not uploaded on the server?
    /// </summary>
    public static bool IsLocalChanged(this PassFile passFile) 
        => passFile.IsLocalCreated() ||
           passFile.IsLocalDeleted() ||
           passFile.IsLocalInfoFieldsChanged() ||
           passFile.IsLocalVersionFieldsChanged();

    /// <summary>
    /// Is passfile information changed locally, but not uploaded on the server?
    /// </summary>
    public static bool IsLocalInfoFieldsChanged(this PassFile passFile) 
        => passFile.OriginChangeStamps is null ||
           passFile.OriginChangeStamps.InfoChangedOn != passFile.InfoChangedOn;

    /// <summary>
    /// Is passfile version changed locally, but not uploaded on the server?
    /// </summary>
    public static bool IsLocalVersionFieldsChanged(this PassFile passFile) 
        => passFile.OriginChangeStamps is null ||
           passFile.OriginChangeStamps.VersionChangedOn != passFile.VersionChangedOn;
}