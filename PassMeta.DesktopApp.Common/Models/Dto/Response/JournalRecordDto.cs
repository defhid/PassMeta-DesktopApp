using System;

namespace PassMeta.DesktopApp.Common.Models.Dto.Response;

/// <summary>
/// DTO of a record from remote journal.
/// </summary>
public class JournalRecordDto
{
    /// <summary></summary>
    public JournalRecordDto()
    {
        Kind ??= string.Empty;
        UserIp ??= string.Empty;
        More ??= string.Empty;
    }

    /// <summary>
    /// Record identifier.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Record kind.
    /// </summary>
    public string Kind { get; set; }
        
    /// <summary>
    /// Id of the user that performed the recorded request.
    /// </summary>
    public long? UserId { get; init; }

    /// <summary>
    /// Ip of the user that performed the recorded request.
    /// </summary>
    public string UserIp { get; init; }

    /// <summary>
    /// Login of the user that performed the recorded request.
    /// </summary>
    public string? UserLogin { get; init; }
        
    /// <summary>
    /// Affected passfile id.
    /// </summary>
    public long? AffectedPassFileId { get; init; }

    /// <summary>
    /// Affected passfile name.
    /// </summary>
    public string? AffectedPassFileName { get; init; }

    /// <summary>
    /// Additional information.
    /// </summary>
    public string More { get; init; }
        
    /// <summary>
    /// Record date and time.
    /// </summary>
    public DateTime WrittenOn { get; init; }
}