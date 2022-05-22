namespace PassMeta.DesktopApp.Common.Utils.Extensions
{
    using System;
    using Constants;
    using Enums;

    /// <summary>
    /// Extension methods for <see cref="PassFileType"/>.
    /// </summary>
    public static class PassFileTypeExtensions
    {
        /// <summary>
        /// Get standard encrypted passfile data file extension.
        /// </summary>
        /// <returns><see cref="ExternalFormat.FullExtension"/></returns>
        public static string ToFileExtension(this PassFileType passFileType)
            => (passFileType switch
            {
                PassFileType.Pwd => ExternalFormat.PwdPassfileEncrypted,
                PassFileType.Txt => ExternalFormat.TxtPassfileEncrypted,
                _ => throw new ArgumentOutOfRangeException(nameof(passFileType), passFileType, null)
            }).FullExtension;
    }
}