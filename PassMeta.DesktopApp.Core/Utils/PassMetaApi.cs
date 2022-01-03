namespace PassMeta.DesktopApp.Core.Utils
{
    using DesktopApp.Common;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models;
    
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Common.Interfaces;
    using Newtonsoft.Json;
    using Splat;
    
    /// <summary>
    /// Utility for making requests to PassMeta server.
    /// </summary>
    public static class PassMetaApi
    {
        static PassMetaApi()
        {
            ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;
        }

        private static ILogService Logger => Locator.Current.GetService<ILogService>()!;
        private static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;
        private static IOkBadService OkBadService => Locator.Current.GetService<IOkBadService>()!;

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
        /// Build PUT-request.
        /// </summary>
        public static Request Put(string url, object? data = null) => new("PUT", url, data);
        
        /// <summary>
        /// Build DELETE-request.
        /// </summary>
        public static Request Delete(string url, object? data = null) => new("DELETE", url, data);

        /// <summary>
        /// Does application have a connection to PassMeta server?
        /// </summary>
        /// <remarks>
        /// Updates <see cref="AppConfig"/>
        /// if connection has been found, but <see cref="AppConfig.ServerVersion"/> is null.
        /// </remarks>
        public static async Task<bool> CheckConnectionAsync()
        {
            bool has;
            try
            {
                var request = WebRequest.CreateHttp(AppConfig.Current.ServerUrl + "/check");
                var response = (HttpWebResponse) await request.GetResponseAsync();

                has = response.StatusCode == HttpStatusCode.OK;
            }
            catch (WebException)
            {
                has = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Connection checking error");
                has = false;
            }

            if (has && AppConfig.Current.ServerVersion is null)
            {
                await AppConfig.Current.RefreshFromServerAsync();
            }

            return has;
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
            catch (UriFormatException ex)
            {
                Logger.Error(ex, AppConfig.Current.ServerUrl + url);
                DialogService.ShowError(Resources.API__URL_ERR);
                return null;
            }
            catch (Exception ex)
            {
                DialogService.ShowError(ex.Message);
                return null;
            }
        }

        private static HttpWebRequest? _CreateRequestWithData<TData>(string url, string method, TData data)
        {
            try
            {
                var request = WebRequest.CreateHttp(AppConfig.Current.ServerUrl + url);
                var dataBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));

                request.Method = method;
                request.ContentType = "application/json";
                request.ContentLength = dataBytes.Length;

                using var dataStream = request.GetRequestStream();
                dataStream.Write(dataBytes, 0, dataBytes.Length);

                return request;
            }
            catch (UriFormatException ex)
            {
                Logger.Error(ex, AppConfig.Current.ServerUrl + url);
                DialogService.ShowError(Resources.API__URL_ERR);
                return null;
            }
            catch (Exception ex)
            {
                DialogService.ShowError(ex.Message);
                return null;
            }
        }

        private static async Task<TResponse?> _ExecuteRequest<TResponse>(HttpWebRequest request, string? context, IMapper? badWhatMapper)
            where TResponse : OkBadResponse
        {
            string responseBody;
            string? errMessage;

            request.CookieContainer = new CookieContainer();
            foreach (var (name, value) in AppConfig.Current.Cookies)
            {
                request.CookieContainer.Add(new Cookie(name, value, null, AppConfig.Current.Domain));
            }

            try
            {
                var response = (HttpWebResponse) await request.GetResponseAsync();

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
                
                AppConfig.Current.RefreshCookies(response.Cookies);
            }
            catch (WebException ex)
            {
                errMessage = ex.Status switch
                {
                    WebExceptionStatus.ConnectFailure => Resources.API__CONNECTION_ERR,
                    WebExceptionStatus.Timeout => Resources.API__CONNECTION_TIMEOUT_ERR,
                    _ => ex.Message
                };
                
                var more = ReferenceEquals(errMessage, ex.Message) ? null : ex.Message;

                try
                {
                    await using var stream = ex.Response!.GetResponseStream();
                    using var reader = new StreamReader(stream);

                    responseBody = await reader.ReadToEndAsync();
                    more = responseBody;
                }
                catch
                {
                    // ignored
                }

                DialogService.ShowError(errMessage, context, more);
                return null;
            }

            try
            {
                var data = JsonConvert.DeserializeObject<TResponse>(responseBody);
                if (errMessage is null)
                {
                    if (data is null)
                        throw new FormatException();
                    
                    if (badWhatMapper is not null && !data.Success)
                        OkBadService.ShowResponseFailure(data, badWhatMapper);
                    
                    return data;
                }

                DialogService.ShowError(errMessage, context, responseBody);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, request.RequestUri.ToString());
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

            private IMapper? _handleBad;

            /// <summary></summary>
            public Request(string method, string url, object? data)
            {
                _data = data;
                _url = url;
                _method = method;
                _context = null;
                _handleBad = null;
            }
            
            /// <summary>
            /// Enable bad response handling (failure auto-showing).
            /// </summary>
            public Request WithBadHandling(IMapper? whatMapper = null)
            {
                _handleBad = whatMapper;
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
                    : _ExecuteRequest<OkBadResponse>(request, _context, _handleBad);
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
                    : _ExecuteRequest<OkBadResponse<TResponseData>>(request, _context, _handleBad);
            }
        }
    }
}