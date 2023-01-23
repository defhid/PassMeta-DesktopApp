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
    public int Id { get; init; }
    
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
    /// Content version.
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; init; }

    /// <summary>
    /// Timestamp of creation.
    /// </summary>
    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; init; }

    /// <summary>
    /// Timestamp of information change.
    /// </summary>
    [JsonPropertyName("i_changed_on")]
    public DateTime InfoChangedOn { get; init; }

    /// <summary>
    /// Timestamp of data change.
    /// </summary>
    [JsonPropertyName("v_changed_on")]
    public DateTime VersionChangedOn { get; init; }

    /// <summary>
    /// Origin remote passfile information. 
    /// </summary>
    [JsonPropertyName("origin")]
    public PassFileLocalDto? Origin { get; init; }
}