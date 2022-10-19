namespace PassMeta.DesktopApp.Common.Abstractions
{
    /// <summary>
    /// Application mutable configuration.
    /// </summary>
    public interface IAppConfig
    {
        /// <summary>
        /// Application language code.
        /// </summary>
        public string CultureCode { get; }

        /// <summary>
        /// PassMeta server API. Non-empty string or null.
        /// </summary>
        public string? ServerUrl { get; }

        /// <summary>
        /// Hide user passwords.
        /// </summary>
        public bool HidePasswords { get; }

        /// <summary>
        /// Development mode.
        /// </summary>
        public bool DevMode { get; }

        /// <summary>
        /// Default length for password generator.
        /// </summary>
        public int DefaultPasswordLength { get; }
    }
}