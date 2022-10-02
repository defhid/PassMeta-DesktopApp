namespace PassMeta.DesktopApp.Common.Models.Entities.Extra
{
    using System;

    /// <summary>
    /// Passfile mark.
    /// </summary>
    [Flags]
    public enum PassFileMark
    {
        /// No marks
        None = 0,
        
        /// Passfile is merged
        Merged = 1,
    }
}