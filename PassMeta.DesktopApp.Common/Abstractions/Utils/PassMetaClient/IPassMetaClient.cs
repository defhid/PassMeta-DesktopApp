namespace PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;

using System;
using System.Threading.Tasks;

/// <summary>
/// HTTP PassMeta API client.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IPassMetaClient : IDisposable
{
    /// <summary>
    /// Corresponds to the last call of <see cref="CheckConnectionAsync"/>.
    /// </summary>
    bool Online { get; }

    /// <summary>
    /// Represents <see cref="Online"/>.
    /// </summary>
    IObservable<bool> OnlineObservable { get; }

    /// <summary>
    /// Start building request.
    /// </summary>
    IRequestBuilder Begin(IHttpRequestBase httpRequestBase);

    /// <summary>
    /// Start building request with body support.
    /// </summary>
    IRequestWithBodySupportBuilder Begin(IHttpRequestWithBodySupportBase httpRequestBase);

    /// <summary>
    /// Does application have a connection to PassMeta server.
    /// </summary>
    /// <param name="reset">Set <see cref="Online"/> to false, then check connection.</param>
    Task<bool> CheckConnectionAsync(bool reset = false);
}