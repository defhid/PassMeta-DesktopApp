using System;
using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Dto.Response;

/// <summary>
/// Passfile version DTO.
/// </summary>
public class PassFileVersionDto
{
    /// <summary>
    /// Version number
    /// </summary>
    [JsonProperty("version")]
    public int Version { get; set; }

    /// <summary>
    /// Version timestamp.
    /// </summary>
    [JsonProperty("version_date")]
    public DateTime VersionDate { get; set; }
}