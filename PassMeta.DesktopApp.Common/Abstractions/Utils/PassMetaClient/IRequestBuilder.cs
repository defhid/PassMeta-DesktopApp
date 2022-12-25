namespace PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient
{
    using System.Threading.Tasks;
    using Mapping;
    using Models;

    /// <summary>
    /// PassMeta client request builder.
    /// </summary>
    public interface IRequestBuilder
    {
        /// <summary>
        /// Add response <see cref="OkBadResponse.What"/> mapping.
        /// </summary>
        /// <returns>this.</returns>
        IRequestBuilder WithBadMapping(IMapper<string, string> whatMapper);

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
}