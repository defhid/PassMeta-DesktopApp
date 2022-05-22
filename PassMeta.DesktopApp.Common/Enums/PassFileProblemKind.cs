namespace PassMeta.DesktopApp.Common.Enums
{
    /// <summary>
    /// Kinds of passfile problem.
    /// </summary>
    public enum PassFileProblemKind
    {
        /// <summary>
        /// Passfile needs merge with remote version.
        /// </summary>
        NeedsMerge = 1,
        
        /// <summary>
        /// Passfile wasn't downloaded from the server because of some error.
        /// </summary>
        DownloadingError = 2,
        
        /// <summary>
        /// Passfile wasn't uploaded to the server because of some error.
        /// </summary>
        UploadingError = 3,
        
        /// <summary>
        /// Passfile wasn't deleted from the server because of some error.
        /// </summary>
        RemoteDeletingError = 4,
        
        /// <summary>
        /// Other passfile problem.
        /// </summary>
        Other = 5
    }
}