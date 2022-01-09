namespace PassMeta.DesktopApp.Core.Utils.Extensions
{
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
        public static bool IsInformationDifferentFrom(this PassFile left, PassFile right)
        {
            return left.Id != right.Id ||
                   left.Name != right.Name ||
                   left.Color != right.Color;
        }
        
        /// <summary>
        /// Does passfile version differs from other?
        /// </summary>
        public static bool IsVersionDifferentFrom(this PassFile left, PassFile right)
        {
            return left.Version != right.Version;
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
        /// Get title for passfile, depending on its current state.
        /// </summary>
        public static string GetTitle(this PassFile passFile)
        {
            if (passFile.LocalCreated) 
                return string.Format(Resources.PASSFILE__TITLE_NEW, passFile.Name);
            
            if (passFile.LocalDeleted) 
                return string.Format(Resources.PASSFILE__TITLE_DELETED, passFile.Name, passFile.Id);
            
            return string.Format(Resources.PASSFILE__TITLE, passFile.Name, passFile.Id);
        }
    }
}