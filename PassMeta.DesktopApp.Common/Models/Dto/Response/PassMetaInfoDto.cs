using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Common.Models.Dto.Response;

/// <summary>
/// PassMeta server info DTO.
/// </summary>
public class PassMetaInfoDto
{
    /// <summary>
    /// Current user.
    /// </summary>
    public User? User { get; init; }

    /// <summary>
    /// Server identifier.
    /// </summary>
    public string? AppId { get; init; }

    /// <summary>
    /// Server version.
    /// </summary>
    public string? AppVersion { get; init; }
}