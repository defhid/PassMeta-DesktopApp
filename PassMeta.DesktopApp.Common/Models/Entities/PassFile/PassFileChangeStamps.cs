using System;
using PassMeta.DesktopApp.Common.Abstractions.Entities;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile;

/// <inheritdoc />
public class PassFileChangeStamps : IPassFileChangeStamps
{
    /// <inheritdoc />
    public DateTime InfoChangedOn { get; init; }

    /// <inheritdoc />
    public DateTime VersionChangedOn { get; init; }

    /// <inheritdoc />
    public int Version { get; init; }
}