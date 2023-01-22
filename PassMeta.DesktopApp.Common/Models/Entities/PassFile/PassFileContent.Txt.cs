using System.Collections.Generic;
using System.Linq;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile;

/// <summary>
/// Content for <see cref="PassFileType.Txt"/> passfiles.
/// </summary>
public sealed class TxtPassFileContent : PassFileContent<List<TxtSection>>
{
    /// <summary></summary>
    public static readonly TxtPassFileContent Unloaded = new();

    private TxtPassFileContent()
        : base(null, null, null)
    {
    }

    /// <summary>
    /// Initialize content with encrypted data.
    /// </summary>
    public TxtPassFileContent(byte[] dataEncrypted)
        : base(null, dataEncrypted, null)
    {
    }

    /// <summary>
    /// Initialize content with decrypted data and passphrase to encrypt it.
    /// </summary>
    public TxtPassFileContent(List<TxtSection> dataDecrypted, string passPhrase)
        : base(dataDecrypted, null, passPhrase)
    {
    }

    /// <summary>
    /// Initialize content with decrypted data, encrypted data and passphrase to encrypt and decrypt them.
    /// </summary>
    public TxtPassFileContent(List<TxtSection> dataDecrypted, byte[] dataEncrypted, string passPhrase)
        : base(dataDecrypted, dataEncrypted, passPhrase)
    {
    }

    /// <inheritdoc />
    public override List<TxtSection>? CloneDecryptedData()
    {
        if (DataDecrypted is null)
        {
            return null;
        }

        var clone = new List<TxtSection>(DataDecrypted.Count);
        clone.AddRange(DataDecrypted.Select(x => x.Copy()));
        return clone;
    }
}