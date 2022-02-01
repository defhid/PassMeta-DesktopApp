namespace PassMeta.DesktopApp.Common.Constants
{
    using System;

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
        /// Encrypted passfile format.
        /// </summary>
        public static readonly ExternalFormat PassfileEncrypted =
            new(() => Resources.PASSFILE__EXPORT_PASSFILE, "passfile");
        
        /// <summary>
        /// Open JSON passfile format.
        /// </summary>
        public static readonly ExternalFormat PassfileDecrypted =
            new(() => Resources.PASSFILE__EXPORT_PASSFILE_OPEN, "o-passfile");
    }
}