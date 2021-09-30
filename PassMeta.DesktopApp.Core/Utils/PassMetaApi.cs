using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models;
using Splat;

namespace PassMeta.DesktopApp.Core.Utils
{
    public static class PassMetaApi
    {
        static PassMetaApi()
        {
            ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;
        }
        
        public static async Task<OkBadResponse<TData>?> GetAsync<TData>(string url, 
            bool handleBad = false)
        {
            var request = _CreateRequest(url, "GET");
            if (request is null) return null;

            return await _ExecuteRequest<OkBadResponse<TData>>(request, handleBad);
        }
        
        public static async Task<OkBadResponse<TResponseData>?> PostAsync<TPostData, TResponseData>(string url, 
            TPostData data, bool handleBad = false)
        {
            var request = await _CreateRequestWithDataAsync(url, "POST", data);
            if (request is null) return null;

            return await _ExecuteRequest<OkBadResponse<TResponseData>>(request, handleBad);
        }
        
        public static async Task<OkBadResponse<TResponseData>?> PatchAsync<TPatchData, TResponseData>(string url, 
            TPatchData data, bool handleBad = false)
        {
            var request = await _CreateRequestWithDataAsync(url, "PATCH", data);
            if (request is null) return null;

            return await _ExecuteRequest<OkBadResponse<TResponseData>>(request, handleBad);
        }
        
        public static async Task<OkBadResponse<TResponseData>?> PutAsync<TResponseData>(string url, bool handleBad = false)
        {
            var request = _CreateRequest(url, "PUT");
            if (request is null) return null;

            return await _ExecuteRequest<OkBadResponse<TResponseData>>(request, handleBad);
        }
        
        public static async Task<OkBadResponse<TResponseData>?> DeleteAsync<TResponseData>(string url, bool handleBad = false)
        {
            var request = _CreateRequest(url, "DELETE");
            if (request is null) return null;

            return await _ExecuteRequest<OkBadResponse<TResponseData>>(request, handleBad);
        }

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
                Locator.Current.GetService<IDialogService>()!.ShowError(Resources.ERR__URL);
                return null;
            }
            catch (Exception ex)
            {
                Locator.Current.GetService<IDialogService>()!.ShowError(ex.Message);
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
                Locator.Current.GetService<IDialogService>()!.ShowError(ex.Message);
                return null;
            }
        }

        private static async Task<TResponse?> _ExecuteRequest<TResponse>(HttpWebRequest request, bool handleBad)
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
                    await using var stream = ex.Response.GetResponseStream();
                    using var reader = new StreamReader(stream);

                    responseBody = await reader.ReadToEndAsync();
                    more = responseBody;
                }
                catch
                {
                    // ignored
                }
                
                Locator.Current.GetService<IDialogService>()!.ShowError(errMessage, more: more);
                return null;
            }

            try
            {
                var data = JsonConvert.DeserializeObject<TResponse>(responseBody);
                if (errMessage is null)
                {
                    if (data is null)
                        throw new FormatException();
                    
                    if (handleBad && !data.Success)
                        Locator.Current.GetService<IOkBadService>()!.ShowResponseFailure(data);
                    
                    return data;
                }

                if (data is null)
                    Locator.Current.GetService<IDialogService>()!.ShowError(errMessage);
                else
                    Locator.Current.GetService<IOkBadService>()!.ShowResponseFailure(data);
            }
            catch
            {
                Locator.Current.GetService<IDialogService>()!
                    .ShowError(errMessage ?? Resources.ERR__INVALID_API_RESPONSE, null, responseBody);
            }

            return null;
        }
    }
}