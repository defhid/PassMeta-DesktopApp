using System;

namespace PassMeta.DesktopApp.Common.Models.Dto.Response;

/// <summary>
/// Passfile version DTO.
/// </summary>
public class PassFileVersionDto
{
    /// <summary>
    /// Version number
    /// </summary>
    public int Version { get; init; }

    /// <summary>
    /// Version timestamp.
    /// </summary>
    public DateTime VersionDate { get; init; }
}