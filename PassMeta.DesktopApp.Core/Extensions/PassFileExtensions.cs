using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="PassFile{TContent}"/>.
/// </summary>
public static class PassFileExtensions
{
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
    /// Set information fields from other <paramref name="passFile"/>.
    /// </summary>
    public static void RefreshInfoFieldsFrom(this PassFile passFile, PassFile otherPassFile)
    {
        passFile.Name = otherPassFile.Name;
        passFile.Color = otherPassFile.Color;
        passFile.OriginChangeStamps = otherPassFile.OriginChangeStamps?.Clone();  // TODO: Hm...
        passFile.InfoChangedOn = otherPassFile.InfoChangedOn;
        passFile.LocalDeletedOn = otherPassFile.LocalDeletedOn;
    }
        
    /// <summary>
    /// Set data fields from other <paramref name="passFile"/>.
    /// </summary>
    public static void RefreshDataFieldsFrom(this PassFile passFile, PassFile otherPassFile, bool refreshDecryptedData)
    {
        if (refreshDecryptedData)
        {
            passFile.WithDecryptedContentFrom(otherPassFile);
        }
        passFile.ContentEncrypted = otherPassFile.ContentEncrypted;
        passFile.PassPhrase = otherPassFile.PassPhrase;
        passFile.OriginChangeStamps = otherPassFile.OriginChangeStamps?.Clone();
        passFile.Version = otherPassFile.Version;
        passFile.VersionChangedOn = otherPassFile.VersionChangedOn;
    }
}