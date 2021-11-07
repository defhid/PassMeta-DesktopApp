namespace PassMeta.DesktopApp.Common.Enums
{
    using Models.Entities;

    /// <summary>
    /// Kind of <see cref="PassFileProblem"/>.
    /// </summary>
    public enum PassFileProblemKind
    {
        NeedsMerge = 1,
        DownloadingError = 2,
        UploadingError = 3,
        Other = 4
    }
}