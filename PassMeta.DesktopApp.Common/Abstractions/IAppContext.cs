namespace PassMeta.DesktopApp.Common.Abstractions
{
    using System.Collections.Generic;
    using System.Net;
    using Models.Entities;

    /// <summary>
    /// Application context: cookies, user, etc.
    /// </summary>
    public interface IAppContext
    {
        /// <summary>
        /// Cookies from the server.
        /// </summary>
        public List<Cookie> Cookies { get; set; }

        /// <summary>
        /// Application user.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Total count of locally created passfiles.
        /// </summary>
        public uint PassFilesCounter { get; set; }

        /// <summary>
        /// Server identifier.
        /// </summary>
        public string? ServerId { get; }

        /// <summary>
        /// Server version. If not null, indicates correct <see cref="IAppConfig.ServerUrl"/>
        /// and internet connection has been established at least once.
        /// </summary>
        public string? ServerVersion { get; }

        /// <summary>
        /// <see cref="Cookies"/> in form of <see cref="System.Net.CookieContainer"/>.
        /// </summary>
        public CookieContainer CookieContainer { get; }
    }
}