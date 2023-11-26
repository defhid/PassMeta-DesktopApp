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
 
    /// <summary>
    /// Get passfile short identity string (id + name).
    /// </summary>
    public static string GetIdentityString(this PassFile passFile)
        => $"#{passFile.Id.ToString().Replace('-', '~')} '{passFile.Name.Replace("'", "")}'";

    /// <summary>
    /// Get title for passfile, depending on its current state.
    /// </summary>
    public static string GetTitle(this PassFile passFile)
    {
        if (passFile.IsLocalCreated())
            return string.Format(Resources.PASSFILE__TITLE_NEW, passFile.GetIdentityString());

        if (passFile.IsLocalDeleted())
            return string.Format(Resources.PASSFILE__TITLE_DELETED, passFile.GetIdentityString());

        return string.Format(Resources.PASSFILE__TITLE, passFile.GetIdentityString());
    }

    /// <summary>
    /// Copy information fields from <paramref name="sourcePassFile"/>.
    /// </summary>
    public static void LoadInfoFieldsFrom(this PassFile passFile, PassFile sourcePassFile)
    {
        passFile.Name = sourcePassFile.Name;
        passFile.Color = sourcePassFile.Color;
        passFile.InfoChangedOn = sourcePassFile.InfoChangedOn;
    }
    
    /// <summary>
    /// Copy version fields from <paramref name="sourcePassFile"/>.
    /// </summary>
    public static void LoadVersionFieldsFrom(this PassFile passFile, PassFile sourcePassFile)
    {
        passFile.Version = sourcePassFile.Version;
        passFile.VersionChangedOn = sourcePassFile.VersionChangedOn;
    }
}