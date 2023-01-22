using System;
using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Dto.Response;

/// <summary>
/// DTO of a record from remote journal.
/// </summary>
public class JournalRecordDto
{
    /// <summary>
    /// Record identifier.
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; init; }

    /// <summary>
    /// Record kind.
    /// </summary>
    [JsonProperty("kind")]
    public string Kind { get; set; } = null!;
        
    /// <summary>
    /// Id of the user that performed the recorded request.
    /// </summary>
    [JsonProperty("user_id")]
    public long? UserId { get; init; }

    /// <summary>
    /// Ip of the user that performed the recorded request.
    /// </summary>
    [JsonProperty("user_ip")]
    public string UserIp { get; init; } = null!;

    /// <summary>
    /// Login of the user that performed the recorded request.
    /// </summary>
    [JsonProperty("user_login")]
    public string? UserLogin { get; init; }
        
    /// <summary>
    /// Affected passfile id.
    /// </summary>
    [JsonProperty("affected_passfile_id")]
    public long? AffectedPassFileId { get; init; }

    /// <summary>
    /// Affected passfile name.
    /// </summary>
    [JsonProperty("affected_passfile_name")]
    public string? AffectedPassFileName { get; init; }

    /// <summary>
    /// Additional information.
    /// </summary>
    [JsonProperty("more")]
    public string More { get; init; } = null!;
        
    /// <summary>
    /// Record date and time.
    /// </summary>
    [JsonProperty("written_on")]
    public DateTime WrittenOn { get; init; }
}