namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;

/// <summary>
/// Service for providing random generation.
/// </summary>
public interface IPassMetaRandomService
{
    /// <summary>
    /// Generate random password by length, using digits and special symbols.
    /// </summary>
    /// <returns>Generated string.</returns>
    string GeneratePassword(int length, bool digits, bool lowercase, bool uppercase, bool special);
}