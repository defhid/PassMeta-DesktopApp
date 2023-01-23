using System;

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
    public int Id { get; init; }
    
    /// <summary>
    /// Identifier of owner user.
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// Identifier of content type.
    /// </summary>
    public int TypeId { get; init; }
    
    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Distinctive color (HEX).
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Content version.
    /// </summary>
    public int Version { get; init; }

    /// <summary>
    /// Timestamp of creation.
    /// </summary>
    public DateTime CreatedOn { get; init; }

    /// <summary>
    /// Timestamp of information change.
    /// </summary>
    public DateTime InfoChangedOn { get; init; }

    /// <summary>
    /// Timestamp of data change.
    /// </summary>
    public DateTime VersionChangedOn { get; init; }
}