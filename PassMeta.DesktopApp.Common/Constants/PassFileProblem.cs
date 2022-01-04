namespace PassMeta.DesktopApp.Common.Constants
{
    using System;
    using Enums;

    /// <summary>
    /// Passfile problem (error info, waiting action, etc.).
    /// </summary>
    public class PassFileProblem
    {
        /// <inheritdoc cref="PassFileProblemKind"/>
        public readonly PassFileProblemKind Kind;

        /// <summary>
        /// Additional information.
        /// </summary>
        public string Info;

        /// <summary>
        /// Payload to solve the problem.
        /// </summary>
        public object? More;
        
        /// <summary>
        /// Localized name of the problem.
        /// </summary>
        public string Name => _nameGetter();
        
        private readonly Func<string> _nameGetter;

        private PassFileProblem(PassFileProblemKind kind, Func<string> nameGetter)
        {
            _nameGetter = nameGetter;
            Kind = kind;
            Info = string.Empty;
            More = null;
        }

        /// <summary>
        /// Set info and return this.
        /// </summary>
        public PassFileProblem WithInfo(string? info)
        {
            Info = info ?? string.Empty;
            return this;
        }
        
        /// <summary>
        /// Set more and return this.
        /// </summary>
        public PassFileProblem WithMore(object? more)
        {
            More = more;
            return this;
        }

        #region Variants

        /// <inheritdoc cref="PassFileProblemKind.NeedsMerge"/>
        public static readonly PassFileProblem NeedsMerge = 
            new(PassFileProblemKind.NeedsMerge, () => Resources.PASSFILE_PROBLEM__NEEDS_MERGE);
        
        /// <inheritdoc cref="PassFileProblemKind.DownloadingError"/>
        public static readonly PassFileProblem DownloadingError = 
            new(PassFileProblemKind.DownloadingError, () => Resources.PASSFILE_PROBLEM__DOWNLOAD_ERR);
        
        /// <inheritdoc cref="PassFileProblemKind.UploadingError"/>
        public static readonly PassFileProblem UploadingError = 
            new(PassFileProblemKind.UploadingError, () => Resources.PASSFILE_PROBLEM__UPLOAD_ERR);
        
        /// <inheritdoc cref="PassFileProblemKind.Other"/>
        public static readonly PassFileProblem RemoteDeletingError = 
            new(PassFileProblemKind.RemoteDeletingError, () => Resources.PASSFILE_PROBLEM__REMOTE_DELETING_ERR);
        
        /// <inheritdoc cref="PassFileProblemKind.Other"/>
        public static readonly PassFileProblem Other = 
            new(PassFileProblemKind.Other, () => Resources.PASSFILE_PROBLEM__OTHER);

        #endregion
    }
}