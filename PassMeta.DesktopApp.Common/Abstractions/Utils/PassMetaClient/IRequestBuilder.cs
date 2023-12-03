using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models.Dto.Response;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;

/// <summary>
/// PassMeta client request builder.
/// </summary>
public interface IRequestBuilder
{
    /// <summary>
    /// Enable bad response handling (failures showing).
    /// </summary>
    /// <returns>this.</returns>
    IRequestBuilder WithBadHandling();

    /// <summary>
    /// Set context for <see cref="WithBadHandling"/>.
    /// </summary>
    /// <returns>this.</returns>
    IRequestBuilder WithContext(string? context);

    /// <summary>
    /// Execute request and return deserialized response content.
    /// </summary>
    /// <returns>this.</returns>
    ValueTask<RestResponse> ExecuteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute request and return deserialized response content.
    /// </summary>
    /// <returns>this.</returns>
    ValueTask<RestResponse<TResponseData>> ExecuteAsync<TResponseData>(CancellationToken cancellationToken = default)
        where TResponseData : class;
}

/// <summary>
/// PassMeta client request with body support builder.
/// </summary>
public interface IRequestWithBodySupportBuilder : IRequestBuilder
{
    /// <summary>
    /// Attach JSON data.
    /// </summary>
    /// <returns>this.</returns>
    IRequestWithBodySupportBuilder WithJsonBody(object data);

    /// <summary>
    /// Attach form data.
    /// </summary>
    /// <returns>this.</returns>
    IRequestWithBodySupportBuilder WithFormBody(object data);
}