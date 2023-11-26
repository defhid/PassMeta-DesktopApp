namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile.Extra;

/// <summary>
/// Arguments for creating a passfile.
/// </summary>
public class PassFileCreationArgs
{
    /// <summary>
    /// Passphrase to encrypt/decrypt passfile content.
    /// </summary>
    public readonly string PassPhrase;

    /// <summary></summary>
    public PassFileCreationArgs(string passPhrase)
    {
        PassPhrase = passPhrase;
    }
}