using System.Diagnostics;
using System.Runtime.CompilerServices;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="PassFile{TContent}"/>.
/// </summary>
public static class PassFileExtensions
{
    /// <summary>
    /// Does passfile information differs from other?
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInformationDifferentFrom(this PassFile left, PassFile right)
        => left.Id != right.Id ||
           left.Name != right.Name ||
           left.Color != right.Color;

    /// <summary>
    /// Does passfile version differs from other?
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsVersionDifferentFrom(this PassFile left, PassFile right)
        => left.Version != right.Version || 
           left.VersionChangedOn != right.VersionChangedOn;

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
    
    #region With / without

    /// <summary>
    /// Set <see cref="PassFile{TContent}.ContentEncrypted"/> and <see cref="PassFile{TContent}.PassPhrase"/>
    /// from other passfile.
    /// </summary>
    /// <returns>Refreshed passfile.</returns>
    public static TPassFile WithEncryptedContentFrom<TPassFile>(this TPassFile passFile, TPassFile fromPassFile)
        where TPassFile : PassFile
    {
        passFile.ContentEncrypted = fromPassFile.ContentEncrypted;
        passFile.PassPhrase = fromPassFile.PassPhrase;
        return passFile;
    }
        
    /// <summary>
    /// Set decrypted data field from other passfile according to <see cref="PassFile{TContent}.Type"/>.
    /// </summary>
    /// <returns>Refreshed passfile.</returns>
    public static TPassFile WithDecryptedContentFrom<TPassFile, TContent>(this TPassFile passFile, TPassFile fromPassFile)
        where TPassFile : PassFile<TContent>
        where TContent : class
    {
        Debug.Assert(passFile.Type == fromPassFile.Type);

        passFile.Content = fromPassFile.CloneContent();
        return passFile;
    }
        
    /// <summary>
    /// Set decrypted data field to null according to <see cref="PassFile{TContent}.Type"/>.
    /// </summary>
    /// <returns>Refreshed passfile.</returns>
    public static TPassFile WithoutDecryptedData<TPassFile, TContent>(this TPassFile passFile)
        where TPassFile : PassFile<TContent>
        where TContent : class
    {
        passFile.Content = null;
        return passFile;
    }

    #endregion
}