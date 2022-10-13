namespace PassMeta.DesktopApp.Common.Constants
{
    using System;
    using Enums;

    /// <summary>
    /// Passfile import/export format.
    /// </summary>
    public class ExternalFormat
    {
        /// <summary>
        /// Name of the format.
        /// </summary>
        public string Name => _nameGetter();

        /// <summary>
        /// File extension without dot.
        /// </summary>
        public readonly string PureExtension;

        /// <summary>
        /// File extension with dot.
        /// </summary>
        public readonly string FullExtension;

        private readonly Func<string> _nameGetter;

        /// <summary></summary>
        private ExternalFormat(Func<string> nameGetter, string extension)
        {
            _nameGetter = nameGetter;
            PureExtension = extension;
            FullExtension = '.' + extension;
        }

        /// <summary>
        /// Encrypted <see cref="PassFileType.Pwd"/> passfile format.
        /// </summary>
        public static readonly ExternalFormat PwdPassfileEncrypted =
            new(() => Resources.PASSFILE__EXPORT_PASSFILE, "pfpwd");
        
        /// <summary>
        /// Open JSON <see cref="PassFileType.Pwd"/> passfile format.
        /// </summary>
        public static readonly ExternalFormat PwdPassfileDecrypted =
            new(() => Resources.PASSFILE__EXPORT_PASSFILE_OPEN, "pfpwd-json");

        /// <summary>
        /// Encrypted <see cref="PassFileType.Txt"/> passfile format.
        /// </summary>
        public static readonly ExternalFormat TxtPassfileEncrypted =
            new(() => Resources.PASSFILE__EXPORT_PASSFILE, "pftxt");

        /// <summary>
        /// Open JSON <see cref="PassFileType.Txt"/> passfile format.
        /// </summary>
        public static readonly ExternalFormat TxtPassfileDecrypted =
            new(() => Resources.PASSFILE__EXPORT_PASSFILE_OPEN, "pftxt-json");
    }
}