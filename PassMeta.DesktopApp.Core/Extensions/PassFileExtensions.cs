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
}