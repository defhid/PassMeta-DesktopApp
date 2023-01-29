using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;
using PassMeta.DesktopApp.Common.Models.Dto.Response.OkBad;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;

using System.Threading.Tasks;

/// <summary>
/// PassMeta client request builder.
/// </summary>
public interface IRequestBuilder
{
    /// <summary>
    /// Add response <see cref="OkBadMore.What"/> mapping.
    /// </summary>
    /// <returns>this.</returns>
    IRequestBuilder WithBadMapping(IValuesMapper<string, string> whatValuesMapper);

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
    /// Execute request and return response content bytes.
    /// </summary>
    /// <returns>this.</returns>
    ValueTask<byte[]?> ExecuteRawAsync();

    /// <summary>
    /// Execute request and return deserialized response content.
    /// </summary>
    /// <returns>this.</returns>
    ValueTask<OkBadResponse?> ExecuteAsync();

    /// <summary>
    /// Execute request and return deserialized response content.
    /// </summary>
    /// <returns>this.</returns>
    ValueTask<OkBadResponse<TResponseData>?> ExecuteAsync<TResponseData>();
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