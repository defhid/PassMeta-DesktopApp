namespace PassMeta.DesktopApp.Common.Constants
{
    using System;
    using Enums;

    public class PassFileProblem
    {
        public readonly PassFileProblemKind Kind;

        public string Info;

        public object? More;
        
        public string Name => _nameGetter();
        
        private readonly Func<string> _nameGetter;

        private PassFileProblem(PassFileProblemKind kind, Func<string> nameGetter)
        {
            _nameGetter = nameGetter;
            Kind = kind;
            Info = string.Empty;
            More = null;
        }

        public PassFileProblem WithInfo(string? info)
        {
            Info = info ?? string.Empty;
            return this;
        }
        
        public PassFileProblem WithMore(object? more)
        {
            More = more;
            return this;
        }

        public static readonly PassFileProblem NeedsMerge = 
            new(PassFileProblemKind.NeedsMerge, () => Resources.PASSFILE_PROBLEM__NEEDS_MERGE);
        
        public static readonly PassFileProblem DownloadingError = 
            new(PassFileProblemKind.DownloadingError, () => Resources.PASSFILE_PROBLEM__DOWNLOAD_ERR);
        
        public static readonly PassFileProblem UploadingError = 
            new(PassFileProblemKind.UploadingError, () => Resources.PASSFILE_PROBLEM__UPLOAD_ERR);
        
        public static readonly PassFileProblem Other = 
            new(PassFileProblemKind.Other, () => Resources.PASSFILE_PROBLEM__OTHER);
    }
}