using System;

namespace PassMeta.DesktopApp.Common.Models.Dto.Request;

/// <summary>
/// Passfile creation data.
/// </summary>
public class PassFilePostData
{
    ///
    public string Name { get; init; } = null!;
        
    ///
    public string? Color { get; init; }
        
    ///
    public int TypeId { get; init; }
        
    ///
    public DateTime CreatedOn { get; init; }
        
    ///
    public string Smth { get; init; } = null!;
}