using System;
using Newtonsoft.Json;
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
    [JsonProperty("type")]
    public PassFileType Type { get; init; }

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
    [JsonProperty("i_changed_on")]
    public DateTime InfoChangedOn { get; init; }

    /// <summary>
    /// Timestamp of data change.
    /// </summary>
    [JsonProperty("v_changed_on")]
    public DateTime VersionChangedOn { get; init; }

    /// <summary>
    /// Timestamp of local deletion.
    /// </summary>
    [JsonProperty("l_deleted_on")]
    public DateTime? LocalDeletedOn { get; init; }

    /// <summary>
    /// Origin remote passfile information. 
    /// </summary>
    [JsonProperty("origin")]
    public PassFileLocalDto? Origin { get; init; }
}