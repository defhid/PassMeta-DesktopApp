using System;
using PassMeta.DesktopApp.Common.Utils.ValueMapping;

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
    NeedsMerge = 0b_1,

    /// <summary>
    /// Passfile is merged.
    /// </summary>
    Merged = 0b_10,

    #endregion

    #region Error

    /// <summary>
    /// Passfile wasn't downloaded from the server because of some error.
    /// </summary>
    DownloadingError = 0b_100,

    /// <summary>
    /// Passfile wasn't uploaded to the server because of some error.
    /// </summary>
    UploadingError = 0b_1000,

    /// <summary>
    /// Passfile wasn't deleted from the server because of some error.
    /// </summary>
    RemoteDeletingError = 0b_1000_0,

    /// <summary>
    /// Other passfile error.
    /// </summary>
    OtherError = 0b_1000_00,

    /// <summary>
    /// All passfile error marks.
    /// </summary>
    AllErrors = DownloadingError | UploadingError | RemoteDeletingError | OtherError

    #endregion
}