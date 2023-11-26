namespace PassMeta.DesktopApp.Common.Models.Presets;

/// <summary>
/// Presets for password generator.
/// </summary>
public class PasswordGeneratorPresets
{
    /// <summary>
    /// Length of result password.
    /// </summary>
    public int Length { get; init; }
    
    /// <summary>
    /// Include digits to result password.
    /// </summary>
    public bool IncludeDigits { get; init; }

    /// <summary>
    /// Include lowercase symbols to result password.
    /// </summary>
    public bool IncludeLowercase { get; init; }
    
    /// <summary>
    /// Include lowercase symbols to result password.
    /// </summary>
    public bool IncludeUppercase { get; init; }

    /// <summary>
    /// Include special symbols to result password.
    /// </summary>
    public bool IncludeSpecial { get; init; }
}