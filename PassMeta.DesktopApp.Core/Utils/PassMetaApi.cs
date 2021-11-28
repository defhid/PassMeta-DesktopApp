namespace PassMeta.DesktopApp.Core.Utils
{
    using DesktopApp.Common;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models;
    
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    
    using Newtonsoft.Json;
    using Splat;
    
    public static class PassMetaApi
    {
        static PassMetaApi()
        {
            ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;
        }

        public static Task<OkBadResponse<TResponseData>?> GetAsync<TResponseData>(string url, bool handleBad)
        {
            var request = new Request("GET", url, null);
            
            return (handleBad ? request.WithBadHandling() : request).ExecuteAsync<TResponseData>();
        }

        public static Request Post(string url, object? data = null) => new("POST", url, data);

        public static Request Patch(string url, object? data = null) => new("PATCH", url, data);
        
        public static Request Put(string url, object? data = null) => new("PUT", url, data);
        
        public static Request Delete(string url, object? data = null) => new("DELETE", url, data);

        #region Private

        private static async Task<HttpWebRequest?> _CreateRequestAsync(string url, string method)
        {
            try
            {
                var request = WebRequest.CreateHttp(AppConfig.Current.ServerUrl + url);
                request.Method = method;
                return request;
            }
            catch (UriFormatException)
            {
                await Locator.Current.GetService<IDialogService>()!.ShowErrorAsync(Resources.ERR__URL);
                return null;
            }
            catch (Exception ex)
            {
                await Locator.Current.GetService<IDialogService>()!.ShowErrorAsync(ex.Message);
                return null;
            }
        }

        private static async Task<HttpWebRequest?> _CreateRequestWithDataAsync<TData>(string url, string method, TData data)
        {
            try
            {
                var request = WebRequest.CreateHttp(AppConfig.Current.ServerUrl + url);
                var dataBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));

                request.Method = method;
                request.ContentType = "application/json";
                request.ContentLength = dataBytes.Length;

                await using var dataStream = request.GetRequestStream();
                await dataStream.WriteAsync(dataBytes.AsMemory(0, dataBytes.Length));

                return request;
            }
            catch (Exception ex)
            {
                await Locator.Current.GetService<IDialogService>()!.ShowErrorAsync(ex.Message);
                return null;
            }
        }

        private static async Task<TResponse?> _ExecuteRequest<TResponse>(HttpWebRequest request, IReadOnlyDictionary<string, string>? badWhatMapper)
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
                var response = (HttpWebResponse)request.GetResponse();

                await using var stream = response.GetResponseStream();
                using var reader = new StreamReader(stream);
                
                responseBody = await reader.ReadToEndAsync();

                errMessage = response.StatusCode switch
                {
                    HttpStatusCode.OK => null,
                    HttpStatusCode.Unauthorized => Resources.ERR__HTTP_UNAUTHORIZED,
                    HttpStatusCode.Forbidden => Resources.ERR__HTTP_FORBIDDEN,
                    HttpStatusCode.InternalServerError => Resources.ERR__HTTP_INTERNAL_SERVER,
                    _ => response.StatusCode.ToString()
                };
                
                AppConfig.Current.RefreshCookies(response.Cookies);
            }
            catch (WebException ex)
            {
                errMessage = ex.Status switch
                {
                    WebExceptionStatus.ConnectFailure => Resources.ERR__CONNECTION_FAILURE,
                    WebExceptionStatus.Timeout => Resources.ERR__CONNECTION_TIMEOUT,
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
                
                await Locator.Current.GetService<IDialogService>()!.ShowErrorAsync(errMessage, more: more);
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
                        await Locator.Current.GetService<IOkBadService>()!.ShowResponseFailureAsync(data, badWhatMapper);
                    
                    return data;
                }

                await Locator.Current.GetService<IDialogService>()!.ShowErrorAsync(errMessage, more: responseBody);
            }
            catch
            {
                await Locator.Current.GetService<IDialogService>()!
                    .ShowErrorAsync(errMessage ?? Resources.ERR__INVALID_API_RESPONSE, null, responseBody);
            }

            return null;
        }

        #endregion
        
        public struct Request
        {
            private readonly object? _data;

            private readonly string _url;

            private readonly string _method;

            private IReadOnlyDictionary<string, string>? _handleBad;

            private static readonly IReadOnlyDictionary<string, string> DefaultWhatMapper = new Dictionary<string, string>();

            public Request(string method, string url, object? data)
            {
                _data = data;
                _url = url;
                _method = method;
                _handleBad = null;
            }
            
            public Request WithBadHandling(IReadOnlyDictionary<string, string>? whatMapper = null)
            {
                _handleBad = whatMapper ?? DefaultWhatMapper;
                return this;
            }
            
            public async Task<OkBadResponse?> ExecuteAsync()
            {
                var request = _data is null
                    ? await _CreateRequestAsync(_url, _method)
                    : await _CreateRequestWithDataAsync(_url, _method, _data);
                
                if (request is null) return null;

                return await _ExecuteRequest<OkBadResponse>(request, _handleBad);
            }
            
            public async Task<OkBadResponse<TResponseData>?> ExecuteAsync<TResponseData>()
            {
                var request = _data is null
                    ? await _CreateRequestAsync(_url, _method)
                    : await _CreateRequestWithDataAsync(_url, _method, _data);
                
                if (request is null) return null;

                return await _ExecuteRequest<OkBadResponse<TResponseData>>(request, _handleBad);
            }
        }
    }
}