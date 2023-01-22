using PassMeta.DesktopApp.Common.Models.Entities;
using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Dto.Response;

/// <summary>
/// PassMeta server info DTO.
/// </summary>
public class PassMetaInfoDto
{
    /// <summary>
    /// Current user.
    /// </summary>
    [JsonProperty("user")]
    public User? User { get; init; }

    /// <summary>
    /// Server identifier.
    /// </summary>
    [JsonProperty("app_id")]
    public string? AppId { get; init; }

    /// <summary>
    /// Server version.
    /// </summary>
    [JsonProperty("app_version")]
    public string? AppVersion { get; init; }
}