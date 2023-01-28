using System;
using System.Text.Json.Serialization;
using PassMeta.DesktopApp.Common.Enums;

namespace PassMeta.DesktopApp.Common.Models.Dto.Internal;

/// <summary>
/// Passfile information DTO for local storage.
/// </summary>
public class PassFileLocalDto
{
    /// <summary></summary>
    public PassFileLocalDto()
    {
        Name ??= "?";
    }

    /// <summary>
    /// Identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; init; }
    
    /// <summary>
    /// Identifier of owner user.
    /// </summary>
    [JsonPropertyName("user_id")]
    public int UserId { get; init; }

    /// <summary>
    /// Identifier of content type.
    /// </summary>
    [JsonPropertyName("type")]
    public PassFileType Type { get; init; }

    /// <summary>
    /// Name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; }

    /// <summary>
    /// Distinctive color (HEX).
    /// </summary>
    [JsonPropertyName("color")]
    public string? Color { get; init; }

    /// <summary>
    /// Timestamp of creation.
    /// </summary>
    [JsonPropertyName("cre_on")]
    public DateTime CreatedOn { get; init; }

    /// <summary>
    /// Timestamp of deletion.
    /// </summary>
    [JsonPropertyName("del_on")]
    public DateTime? DeletedOn { get; init; }

    /// <summary>
    /// Timestamp of information change.
    /// </summary>
    [JsonPropertyName("inf_on")]
    public DateTime InfoChangedOn { get; init; }

    /// <summary>
    /// Timestamp of data change.
    /// </summary>
    [JsonPropertyName("ver_on")]
    public DateTime VersionChangedOn { get; init; }

    /// <summary>
    /// Content version.
    /// </summary>
    [JsonPropertyName("ver")]
    public int Version { get; init; }

    /// <summary>
    /// Origin remote passfile information. 
    /// </summary>
    [JsonPropertyName("orig")]
    public PassFileLocalDto? Origin { get; init; }
}