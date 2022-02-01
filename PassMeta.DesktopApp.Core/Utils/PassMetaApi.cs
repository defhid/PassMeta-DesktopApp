namespace PassMeta.DesktopApp.Core.Utils
{
    using DesktopApp.Common;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models;
    
    using System;
    using System.IO;
    using System.Net;
    using System.Reactive.Subjects;
    using System.Text;
    using System.Threading.Tasks;
    using Common.Interfaces.Mapping;
    using Common.Utils.Extensions;
    using Extensions;
    using Newtonsoft.Json;
    
    /// <summary>
    /// Utility for making requests to PassMeta server.
    /// </summary>
    public static class PassMetaApi
    {
        /// <summary>
        /// Corresponds to the last call of <see cref="CheckConnectionAsync"/>.
        /// </summary>
        public static bool Online { get; private set; }

        /// <summary>
        /// Represents <see cref="Online"/>.
        /// </summary>
        public static readonly BehaviorSubject<bool> OnlineSource = new(false);

        static PassMetaApi()
        {
            ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;
            OnlineSource.Subscribe(online => Online = online);
        }

        #region Services

        private static ILogService Logger => EnvironmentContainer.Resolve<ILogService>();
        private static IDialogService DialogService => EnvironmentContainer.Resolve<IDialogService>();
        private static IOkBadService OkBadService => EnvironmentContainer.Resolve<IOkBadService>();

        #endregion

        /// <summary>
        /// Make GET-request and return response.
        /// </summary>
        public static Task<OkBadResponse<TResponseData>?> GetAsync<TResponseData>(string url, bool handleBad)
        {
            var request = new Request("GET", url, null);
            
            return (handleBad ? request.WithBadHandling() : request).ExecuteAsync<TResponseData>();
        }

        /// <summary>
        /// Build POST-request.
        /// </summary>
        public static Request Post(string url, object? data = null) => new("POST", url, data);

        /// <summary>
        /// Build PATCH-request.
        /// </summary>
        public static Request Patch(string url, object? data = null) => new("PATCH", url, data);

        /// <summary>
        /// Build DELETE-request.
        /// </summary>
        public static Request Delete(string url, object? data = null) => new("DELETE", url, data);

        /// <summary>
        /// Does application have a connection to PassMeta server?
        /// </summary>
        /// <remarks>
        /// Updates <see cref="AppContext"/>
        /// if connection has been found, but <see cref="AppContext.ServerVersion"/> is null.
        /// </remarks>
        public static async Task<bool> CheckConnectionAsync(bool showNoConnection = false)
        {
            if (AppConfig.Current.ServerUrl is null) return false;

            bool has;
            try
            {
                var request = WebRequest.CreateHttp(AppConfig.Current.ServerUrl + "/check");
                var response = (HttpWebResponse) await request.GetResponseAsync();

                has = response.StatusCode == HttpStatusCode.OK;
            }
            catch (WebException)
            {
                if (showNoConnection)
                    DialogService.ShowInfo(Resources.API__CONNECTION_ERR);
                
                has = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Connection checking error");
                has = false;
            }

            if (has && AppContext.Current.ServerVersion is null)
            {
                await AppContext.RefreshFromServerAsync(false);
            }

            if (has != Online) OnlineSource.OnNext(has);

            return Online;
        }

        #region Private

        private static HttpWebRequest? _CreateRequest(string url, string method)
        {
            try
            {
                var request = WebRequest.CreateHttp(AppConfig.Current.ServerUrl + url);
                request.Method = method;
                return request;
            }
            catch (UriFormatException)
            {
                DialogService.ShowError(Resources.API__URL_ERR, more: $"{method} {AppConfig.Current.ServerUrl}{url}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Request creation error: {method} {AppConfig.Current.ServerUrl}{url}");
                DialogService.ShowError(ex.Message);
                return null;
            }
        }

        private static HttpWebRequest? _CreateRequestWithData<TData>(string url, string method, TData data)
        {
            try
            {
                var request = WebRequest.CreateHttp(AppConfig.Current.ServerUrl + url);
                var dataBytes = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(data));

                request.Method = method;
                request.ContentType = "application/json";
                request.ContentLength = dataBytes.Length;

                using var dataStream = request.GetRequestStream();
                dataStream.Write(dataBytes, 0, dataBytes.Length);

                return request;
            }
            catch (UriFormatException ex)
            {
                Logger.Error(ex, $"Request creation error: {method} {AppConfig.Current.ServerUrl}{url}");
                DialogService.ShowError(Resources.API__URL_ERR);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Request creation error: {method} {AppConfig.Current.ServerUrl}{url}");
                DialogService.ShowError(ex.Message);
                return null;
            }
        }

        private static async Task<TResponse?> _ExecuteRequest<TResponse>(HttpWebRequest request, string? context, IMapper<string, string> badMapper, bool handleBad)
            where TResponse : OkBadResponse
        {
            TResponse? responseData = null;
            string? responseBody = null;
            string? errMessage;

            try
            {
                request.CookieContainer = AppContext.Current.CookieContainer;

                using var response = (HttpWebResponse)await request.GetResponseAsync();

                await using var stream = response.GetResponseStream();
                using var reader = new StreamReader(stream);

                responseBody = await reader.ReadToEndAsync();

                errMessage = response.StatusCode switch
                {
                    HttpStatusCode.OK => null,
                    HttpStatusCode.Unauthorized => Resources.API__UNAUTHORIZED_ERR,
                    HttpStatusCode.Forbidden => Resources.API__FORBIDDEN_ERR,
                    HttpStatusCode.InternalServerError => Resources.API__INTERNAL_SERVER_ERR,
                    _ => response.StatusCode.ToString()
                };

                AppContext.RefreshCookies(response.Cookies);
            }
            catch (WebException ex)
            {
                try
                {
                    await using var stream = ex.Response!.GetResponseStream();
                    using var reader = new StreamReader(stream);

                    responseBody = await reader.ReadToEndAsync();
                    responseData = JsonConvert.DeserializeObject<TResponse>(responseBody);
                }
                catch
                {
                    // ignored
                }

                if (ex.Status is WebExceptionStatus.ConnectFailure)
                {
                    if (Online) OnlineSource.OnNext(false);
                }
                else if (responseData is not null)
                {
                    responseData.ApplyMapping(badMapper);
                    
                    Logger.Warning($"{responseBody} [{context}]");
                    
                    if (handleBad)
                        OkBadService.ShowResponseFailure(responseData, context);

                    return responseData;
                }
                
                errMessage = ex.Status switch
                {
                    WebExceptionStatus.ConnectFailure => Resources.API__CONNECTION_ERR,
                    WebExceptionStatus.Timeout => Resources.API__CONNECTION_TIMEOUT_ERR,
                    _ => ex.Message
                };

                var more = responseBody ?? (ReferenceEquals(errMessage, ex.Message) ? null : ex.Message);

                Logger.Error(ex, request.GetShortInformation() + $" [{context}]");
                DialogService.ShowError(errMessage, context, request.GetShortInformation().NewLine(more));
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Unknown error, request failed: {request.GetShortInformation()} [{context}]");
                DialogService.ShowError(ex.Message, context);
                return null;
            }

            try
            {
                responseData = JsonConvert.DeserializeObject<TResponse>(responseBody);
                if (errMessage is null)
                {
                    if (responseData is null)
                        throw new FormatException();
                    
                    responseData.ApplyMapping(badMapper);
                    
                    if (handleBad && !responseData.Success)
                        OkBadService.ShowResponseFailure(responseData);
                    
                    return responseData;
                }

                Logger.Error($"{errMessage} {request.GetShortInformation()} {responseBody} [{context}]");
                DialogService.ShowError(errMessage, context, request.GetShortInformation().NewLine(responseBody));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"{request.GetShortInformation()} {responseBody} [{context}]");
                DialogService.ShowError(errMessage ?? Resources.API__INVALID_RESPONSE_ERR, context, responseBody);
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Request builder.
        /// </summary>
        public struct Request
        {
            private readonly object? _data;

            private readonly string _url;

            private readonly string _method;

            private string? _context;

            private IMapper<string, string> _badMapper;

            private bool _handleBad;

            /// <summary></summary>
            public Request(string method, string url, object? data)
            {
                _data = data;
                _url = url;
                _method = method;
                _context = null;
                _badMapper = AppContext.Current.OkBadMessagesMapper;
                _handleBad = false;
            }

            /// <summary>
            /// Add response <see cref="OkBadResponse.What"/> mapping.
            /// </summary>
            public Request WithBadMapping(IMapper<string, string> whatMapper)
            {
                _badMapper += whatMapper;
                return this;
            }
            
            /// <summary>
            /// Enable bad response handling (failure auto-showing).
            /// </summary>
            public Request WithBadHandling()
            {
                _handleBad = true;
                return this;
            }
            
            /// <summary>
            /// Set context (used for errors handling).
            /// </summary>
            public Request WithContext(string? context)
            {
                _context = string.IsNullOrWhiteSpace(context) ? null : context;
                return this;
            }
            
            /// <summary>
            /// Execute request and get response.
            /// </summary>
            public Task<OkBadResponse?> ExecuteAsync()
            {
                var request = _data is null
                    ? _CreateRequest(_url, _method)
                    : _CreateRequestWithData(_url, _method, _data);
                
                return request is null
                    ? Task.FromResult<OkBadResponse?>(null)
                    : _ExecuteRequest<OkBadResponse>(request, _context, _badMapper, _handleBad);
            }
            
            /// <summary>
            /// Execute request and get response with data.
            /// </summary>
            public Task<OkBadResponse<TResponseData>?> ExecuteAsync<TResponseData>()
            {
                var request = _data is null
                    ? _CreateRequest(_url, _method)
                    : _CreateRequestWithData(_url, _method, _data);
                
                return request is null
                    ? Task.FromResult<OkBadResponse<TResponseData>?>(null)
                    : _ExecuteRequest<OkBadResponse<TResponseData>>(request, _context, _badMapper, _handleBad);
            }
        }
    }
}