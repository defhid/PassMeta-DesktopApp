using System.Collections.Generic;
using System.Linq;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile;

/// <summary>
/// Content for <see cref="PassFileType.Pwd"/> passfiles.
/// </summary>
public sealed class PwdPassFileContent : PassFileContent<List<PwdSection>>
{
    /// <summary></summary>
    public static readonly PwdPassFileContent Unloaded = new();

    private PwdPassFileContent()
        : base(null, null, null)
    {
    }

    /// <summary>
    /// Initialize content with encrypted data.
    /// </summary>
    public PwdPassFileContent(byte[] dataEncrypted)
        : base(null, dataEncrypted, null)
    {
    }

    /// <summary>
    /// Initialize content with decrypted data and passphrase to encrypt it.
    /// </summary>
    public PwdPassFileContent(List<PwdSection> dataDecrypted, string passPhrase)
        : base(dataDecrypted, null, passPhrase)
    {
    }

    /// <summary>
    /// Initialize content with decrypted data, encrypted data and passphrase to encrypt and decrypt them.
    /// </summary>
    public PwdPassFileContent(List<PwdSection> dataDecrypted, byte[] dataEncrypted, string passPhrase)
        : base(dataDecrypted, dataEncrypted, passPhrase)
    {
    }
}