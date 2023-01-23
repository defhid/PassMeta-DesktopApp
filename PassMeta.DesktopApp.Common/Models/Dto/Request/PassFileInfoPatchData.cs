namespace PassMeta.DesktopApp.Common.Models.Dto.Request;

/// <summary>
/// Passfile information patch data.
/// </summary>
public class PassFileInfoPatchData
{
    ///
    public string Name { get; init; } = null!;
        
    ///
    public string? Color { get; init; }
}