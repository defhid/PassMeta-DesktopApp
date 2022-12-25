using System.Net.Http;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient
{
    /// <summary>
    /// PassMeta client request base.
    /// </summary>
    public interface IHttpRequestBase
    {
        /// <summary>
        /// PassMeta controller path.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// HTTP method.
        /// </summary>
        HttpMethod Method { get; }
    }

    /// <summary>
    /// PassMeta client request with body support base.
    /// </summary>
    public interface IHttpRequestWithBodySupportBase : IHttpRequestBase
    {
    }
}