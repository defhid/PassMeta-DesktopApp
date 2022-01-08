namespace PassMeta.DesktopApp.Common.Models.Entities.Extra
{
    using Enums;
    using Utils.Mapping;

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
        public string Info { get; private set; }

        /// <summary>
        /// Localized name of the problem.
        /// </summary>
        public string Name => KindToName.Map(Kind);

        /// <summary></summary>
        public PassFileProblem(PassFileProblemKind kind)
        {
            Kind = kind;
            Info = string.Empty;
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
        /// Create <see cref="PassFileProblem"/> from its kind.
        /// </summary>
        public static implicit operator PassFileProblem(PassFileProblemKind kind) => new(kind);

        private static readonly ToStringMapper<PassFileProblemKind> KindToName = new MapToResource<PassFileProblemKind>[]
        {
            new(PassFileProblemKind.NeedsMerge, () => Resources.PASSFILE_PROBLEM__NEEDS_MERGE),
            new(PassFileProblemKind.DownloadingError, () => Resources.PASSFILE_PROBLEM__DOWNLOAD_ERR),
            new(PassFileProblemKind.UploadingError, () => Resources.PASSFILE_PROBLEM__UPLOAD_ERR),
            new(PassFileProblemKind.RemoteDeletingError, () => Resources.PASSFILE_PROBLEM__REMOTE_DELETING_ERR),
            new(PassFileProblemKind.Other, () => Resources.PASSFILE_PROBLEM__OTHER),
        };
    }
}