using System;

namespace PassMeta.DesktopApp.Common.Enums;

/// <summary>
/// Passfile pwd section mark, abnormal state.
/// </summary>
[Flags]
public enum PwdSectionMark : byte
{
    /// <summary>
    /// No marks.
    /// </summary>
    None = 0,

    /// <summary>
    /// Section is currently created, not saved.
    /// </summary>
    Created = 0b_1,
}