using System;

namespace PassMeta.DesktopApp.Common.Models.Dto.Request;

/// <summary>
/// Passfile creation data.
/// </summary>
public class PassFilePostData
{
    ///
    public int TypeId { get; init; }

    ///
    public string Name { get; init; } = null!;

    ///
    public string? Color { get; init; }

    ///
    public DateTime CreatedOn { get; init; }
}