namespace PassMeta.DesktopApp.Core.Utils.Extensions
{
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Common;
    using Common.Models.Entities;

    /// <summary>
    /// Extension methods for <see cref="PassFile"/>.
    /// </summary>
    public static class PassFileExtensions
    {
        /// <summary>
        /// Does passfile information differs from other?
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInformationDifferentFrom(this PassFile left, PassFile right)
        {
            return left.Id != right.Id ||
                   left.Name != right.Name ||
                   left.Color != right.Color ||
                   left.LocalDeletedOn != right.LocalDeletedOn;
        }
        
        /// <summary>
        /// Does passfile version differs from other?
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsVersionDifferentFrom(this PassFile left, PassFile right)
        {
            return left.Version != right.Version || 
                   left.VersionChangedOn != right.VersionChangedOn;
        }
        
        /// <summary>
        /// Has passfile information been changed? (based on <see cref="PassFile.Origin"/>)
        /// </summary>
        public static bool IsInformationChanged(this PassFile passFile)
        {
            var origin = passFile.Origin;
            return origin is not null && 
                   IsInformationDifferentFrom(origin, passFile);
        }
        
        /// <summary>
        /// Has passfile version been changed? (based on <see cref="PassFile.Origin"/>)
        /// </summary>
        public static bool IsVersionChanged(this PassFile passFile)
        {
            var origin = passFile.Origin;
            return origin is not null && 
                   IsVersionDifferentFrom(origin, passFile);
        }

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
            if (passFile.LocalCreated) 
                return string.Format(Resources.PASSFILE__TITLE_NEW, passFile.GetIdentityString());
            
            if (passFile.LocalDeleted) 
                return string.Format(Resources.PASSFILE__TITLE_DELETED, passFile.GetIdentityString());
            
            return string.Format(Resources.PASSFILE__TITLE, passFile.GetIdentityString());
        }

        /// <summary>
        /// Set <see cref="PassFile.DataEncrypted"/> and <see cref="PassFile.PassPhrase"/>
        /// from other passfile.
        /// </summary>
        /// <returns>Refreshed passfile.</returns>
        public static PassFile WithEncryptedDataFrom(this PassFile passFile, PassFile fromPassFile)
        {
            passFile.DataEncrypted = fromPassFile.DataEncrypted;
            passFile.PassPhrase = fromPassFile.PassPhrase;
            return passFile;
        }
        
        /// <summary>
        /// Set information fields from other <paramref name="passFile"/>.
        /// </summary>
        public static void RefreshInfoFieldsFrom(this PassFile passFile, PassFile otherPassFile)
        {
            passFile.Name = otherPassFile.Name;
            passFile.Color = otherPassFile.Color;
            passFile.Origin = otherPassFile.Origin?.Copy(false);
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
                passFile.DataPwd = otherPassFile.DataPwd?.Select(section => section.Copy()).ToList();
            }
            passFile.DataEncrypted = otherPassFile.DataEncrypted;
            passFile.PassPhrase = otherPassFile.PassPhrase;
            passFile.Origin = otherPassFile.Origin?.Copy(false);
            passFile.Version = otherPassFile.Version;
            passFile.VersionChangedOn = otherPassFile.VersionChangedOn;
            passFile.Marks = otherPassFile.Marks;
        }
        
        /// <summary>
        /// Does <paramref name="left"/> section have any difference
        /// with <paramref name="right"/>?
        /// </summary>
        public static bool DiffersFrom(this PassFile.PwdSection left, PassFile.PwdSection right)
        {
            return left.Name != right.Name || 
                   left.Items.Count != right.Items.Count ||
                   left.Items.Any(lItem => 
                       right.Items.All(rItem => rItem.DiffersFrom(lItem)));
        }
        
        /// <summary>
        /// Does <paramref name="left"/> item have any difference
        /// with <paramref name="right"/>?
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool DiffersFrom(this PassFile.PwdSection.PwdItem left, PassFile.PwdSection.PwdItem right)
        {
            return left.Password != right.Password || 
                   left.Comment != right.Comment ||
                   !left.What.All(right.What.Contains);
        }
    }
}