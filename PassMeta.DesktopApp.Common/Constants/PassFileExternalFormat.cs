using System;

namespace PassMeta.DesktopApp.Common.Constants;

/// <summary>
/// Passfile import/export format.
/// </summary>
public class PassFileExternalFormat
{
    /// <summary>
    /// Name of the format.
    /// </summary>
    public string Name => _nameGetter();

    /// <summary>
    /// File extension without dot.
    /// </summary>
    public readonly string Extension;

    private readonly Func<string> _nameGetter;

    /// <summary></summary>
    private PassFileExternalFormat(Func<string> nameGetter, string extension)
    {
        _nameGetter = nameGetter;
        Extension = extension;
    }

    /// <summary>
    /// Encrypted format of passfile content.
    /// </summary>
    public static readonly PassFileExternalFormat Encrypted =
        new(() => Resources.PASSFILE__EXPORT_PASSFILE, "passfile");

    /// <summary>
    /// Public format of passfile content.
    /// </summary>
    public static readonly PassFileExternalFormat Decrypted =
        new(() => Resources.PASSFILE__EXPORT_PASSFILE_OPEN, "json");
}