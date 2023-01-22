using System;
using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Dto.Response;

/// <summary>
/// Passfile information DTO.
/// </summary>
public class PassFileInfoDto
{
    /// <summary></summary>
    public PassFileInfoDto()
    {
        Name ??= "?";
    }

    /// <summary>
    /// Identifier.
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; init; }
    
    /// <summary>
    /// Identifier of owner user.
    /// </summary>
    [JsonProperty("user_id")]
    public int UserId { get; init; }

    /// <summary>
    /// Identifier of content type.
    /// </summary>
    [JsonProperty("type_id")]
    public int TypeId { get; init; }
    
    /// <summary>
    /// Name.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; init; }

    /// <summary>
    /// Distinctive color (HEX).
    /// </summary>
    [JsonProperty("color")]
    public string? Color { get; init; }

    /// <summary>
    /// Content version.
    /// </summary>
    [JsonProperty("version")]
    public int Version { get; init; }

    /// <summary>
    /// Timestamp of creation.
    /// </summary>
    [JsonProperty("created_on")]
    public DateTime CreatedOn { get; init; }

    /// <summary>
    /// Timestamp of information change.
    /// </summary>
    [JsonProperty("info_changed_on")]
    public DateTime InfoChangedOn { get; init; }

    /// <summary>
    /// Timestamp of data change.
    /// </summary>
    [JsonProperty("version_changed_on")]
    public DateTime VersionChangedOn { get; init; }
}