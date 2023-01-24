using System;

namespace PassMeta.DesktopApp.Common.Abstractions.Entities.PassFile;

/// <summary>
/// Passfile with timestamps information.
/// </summary>
public interface IPassFileWithTimestamps : IPassFileBase
{
    /// <summary>
    /// Timestamp of information change.
    /// </summary>
    DateTime InfoChangedOn { get; }

    /// <summary>
    /// Timestamp of content change.
    /// </summary>
    DateTime VersionChangedOn { get; }

    /// <summary>
    /// Content version.
    /// </summary>
    int Version { get; }
}