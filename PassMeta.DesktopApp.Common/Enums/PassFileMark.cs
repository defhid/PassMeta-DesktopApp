using System;
using PassMeta.DesktopApp.Common.Utils.Mapping;

namespace PassMeta.DesktopApp.Common.Enums;

/// <summary>
/// Passfile mark, abnormal state.
/// </summary>
[Flags]
public enum PassFileMark : short
{
    /// <summary>
    /// No marks.
    /// </summary>
    None = 0,

    #region Merge

    /// <summary>
    /// Passfile needs merge with remote version.
    /// </summary>
    NeedsMerge = 0b1,

    /// <summary>
    /// Passfile is merged.
    /// </summary>
    Merged = 0b10,

    #endregion

    #region Error

    /// <summary>
    /// Passfile wasn't downloaded from the server because of some error.
    /// </summary>
    DownloadingError = 0b100,
        
    /// <summary>
    /// Passfile wasn't uploaded to the server because of some error.
    /// </summary>
    UploadingError = 0b1000,
        
    /// <summary>
    /// Passfile wasn't deleted from the server because of some error.
    /// </summary>
    RemoteDeletingError = 0b10000,
        
    /// <summary>
    /// Other passfile problem.
    /// </summary>
    OtherError = 0b100000,

    #endregion
}

public class Dict
{
    private static readonly SimpleMapper<PassFileMark, string> KindToName = new MapToResource<PassFileMark>[]
    {
        new(PassFileMark.NeedsMerge, () => Resources.PASSFILE_PROBLEM__NEEDS_MERGE),
        new(PassFileMark.Merged, () => Resources.),
        new(PassFileMark.DownloadingError, () => Resources.PASSFILE_PROBLEM__DOWNLOAD_ERR),
        new(PassFileMark.UploadingError, () => Resources.PASSFILE_PROBLEM__UPLOAD_ERR),
        new(PassFileMark.RemoteDeletingError, () => Resources.PASSFILE_PROBLEM__REMOTE_DELETING_ERR),
        new(PassFileMark.OtherError, () => Resources.PASSFILE_PROBLEM__OTHER),
    };
}