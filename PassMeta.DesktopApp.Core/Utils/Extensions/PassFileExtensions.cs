using System.Diagnostics;
using System.Runtime.CompilerServices;

using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.Entities.PassFile;
using PassMeta.DesktopApp.Common.Extensions;

namespace PassMeta.DesktopApp.Core.Utils.Extensions;

/// <summary>
/// Extension methods for <see cref="IPassFile{TContent}"/>.
/// </summary>
public static class PassFileExtensions
{
    /// <summary>
    /// Does passfile information differs from other?
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInformationDifferentFrom(this IPassFile left, IPassFile right)
        => left.Id != right.Id ||
           left.Name != right.Name ||
           left.Color != right.Color;

    /// <summary>
    /// Does passfile version differs from other?
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsVersionDifferentFrom(this IPassFile left, IPassFile right)
        => left.Version != right.Version || 
           left.VersionChangedOn != right.VersionChangedOn;

    /// <summary>
    /// Get passfile short identity string (id + name).
    /// </summary>
    public static string GetIdentityString(this IPassFile passFile) 
        => $"#{passFile.Id.ToString().Replace('-', '~')} '{passFile.Name.Replace("'", "")}'";

    /// <summary>
    /// Get title for passfile, depending on its current state.
    /// </summary>
    public static string GetTitle(this IPassFile passFile)
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
    public static void RefreshInfoFieldsFrom(this IPassFile passFile, IPassFile otherPassFile)
    {
        passFile.Name = otherPassFile.Name;
        passFile.Color = otherPassFile.Color;
        passFile.Origin = otherPassFile.Origin?.Clone();  // TODO: Hm...
        passFile.InfoChangedOn = otherPassFile.InfoChangedOn;
        passFile.LocalDeletedOn = otherPassFile.LocalDeletedOn;
    }
        
    /// <summary>
    /// Set data fields from other <paramref name="passFile"/>.
    /// </summary>
    public static void RefreshDataFieldsFrom(this IPassFile passFile, IPassFile otherPassFile, bool refreshDecryptedData)
    {
        if (refreshDecryptedData)
        {
            passFile.WithDecryptedContentFrom(otherPassFile);
        }
        passFile.ContentEncrypted = otherPassFile.ContentEncrypted;
        passFile.PassPhrase = otherPassFile.PassPhrase;
        passFile.Origin = otherPassFile.Origin?.Clone();
        passFile.Version = otherPassFile.Version;
        passFile.VersionChangedOn = otherPassFile.VersionChangedOn;
    }
    
    #region With / without

    /// <summary>
    /// Set <see cref="IPassFile{TContent}.ContentEncrypted"/> and <see cref="IPassFile{TContent}.PassPhrase"/>
    /// from other passfile.
    /// </summary>
    /// <returns>Refreshed passfile.</returns>
    public static TPassFile WithEncryptedContentFrom<TPassFile>(this TPassFile passFile, TPassFile fromPassFile)
        where TPassFile : IPassFile
    {
        passFile.ContentEncrypted = fromPassFile.ContentEncrypted;
        passFile.PassPhrase = fromPassFile.PassPhrase;
        return passFile;
    }
        
    /// <summary>
    /// Set decrypted data field from other passfile according to <see cref="IPassFile{TContent}.Type"/>.
    /// </summary>
    /// <returns>Refreshed passfile.</returns>
    public static TPassFile WithDecryptedContentFrom<TPassFile, TContent>(this TPassFile passFile, TPassFile fromPassFile)
        where TPassFile : IPassFile<TContent>
        where TContent : class
    {
        Debug.Assert(passFile.Type == fromPassFile.Type);

        passFile.Content = fromPassFile.CloneContent();
        return passFile;
    }
        
    /// <summary>
    /// Set decrypted data field to null according to <see cref="IPassFile{TContent}.Type"/>.
    /// </summary>
    /// <returns>Refreshed passfile.</returns>
    public static TPassFile WithoutDecryptedData<TPassFile, TContent>(this TPassFile passFile)
        where TPassFile : IPassFile<TContent>
        where TContent : class
    {
        passFile.Content = null;
        return passFile;
    }

    #endregion
}