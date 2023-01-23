namespace PassMeta.DesktopApp.Common.Abstractions.Services;

/// <summary>
/// Service for providing passwords generation.
/// </summary>
public interface IPasswordGenerationService
{
    /// <summary>
    /// Generate random password by length, using digits and special symbols.
    /// </summary>
    /// <returns>Generated string.</returns>
    string GeneratePassword(int length, bool digits, bool lowercase, bool uppercase, bool special);
}