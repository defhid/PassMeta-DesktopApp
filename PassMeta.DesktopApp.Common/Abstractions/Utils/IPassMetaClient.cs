namespace PassMeta.DesktopApp.Common.Abstractions.Utils
{
    using System.Threading.Tasks;
    using Mapping;
    using Models;

    /// <summary>
    /// HTTP PassMeta API client.
    /// </summary>
    public interface IPassMetaClient
    {
        /// <summary>
        /// Does application have a connection to PassMeta server.
        /// </summary>
        /// <remarks>
        /// Updates current global <see cref="IAppContext"/>
        /// if connection has been found, but <see cref="IAppContext.ServerVersion"/> is null.
        /// </remarks>
        Task<bool> CheckConnectionAsync(bool showNoConnection = false, bool isFromAppContext = false);
        
        /// <summary>
        /// Start building GET request.
        /// </summary>
        IRequestBuilder Get(string url);

        /// <summary>
        /// Start building POST request.
        /// </summary>
        IRequestWithBodySupportBuilder Post(string url);

        /// <summary>
        /// Start building PATCH request.
        /// </summary>
        IRequestWithBodySupportBuilder Patch(string url);

        /// <summary>
        /// Start building DELETE request.
        /// </summary>
        IRequestWithBodySupportBuilder Delete(string url);

        /// <summary>
        /// HTTP PassMeta API request builder.
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
            ValueTask<IDetailedResult<byte[]>> ExecuteRawAsync();

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
        /// HTTP PassMeta API request with body support builder.
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
}