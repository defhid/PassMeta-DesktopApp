using System;

namespace PassMeta.DesktopApp.Common.Abstractions.Entities;

/// <summary>
/// Information about last passfile changes.
/// </summary>
public interface IPassFileChangeStamps
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