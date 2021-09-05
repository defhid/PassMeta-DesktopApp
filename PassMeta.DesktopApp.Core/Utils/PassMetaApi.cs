using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models;
using Splat;

namespace PassMeta.DesktopApp.Core.Utils
{
    public static class PassMetaApi
    {
        public static async Task<OkBadResponse<TData>> GetAsync<TData>(string url, 
            bool handleBad = false)
        {
            var request = _CreateRequest(url);
            if (request is null) return null;

            return await _ExecuteRequest<OkBadResponse<TData>>(request, handleBad);
        }
        
        public static async Task<OkBadResponse<TResponseData>> PostAsync<TPostData, TResponseData>(string url, 
            TPostData data, bool handleBad = false)
        {
            var request = await _CreateRequestWithDataAsync(url, "POST", data);
            if (request is null) return null;

            return await _ExecuteRequest<OkBadResponse<TResponseData>>(request, handleBad);
        }
        
        public static async Task<OkBadResponse<TResponseData>> PatchAsync<TPatchData, TResponseData>(string url, 
            TPatchData data, bool handleBad = false)
        {
            var request = await _CreateRequestWithDataAsync(url, "PATCH", data);
            if (request is null) return null;

            return await _ExecuteRequest<OkBadResponse<TResponseData>>(request, handleBad);
        }

        private static HttpWebRequest _CreateRequest(string url)
        {
            try
            {
                return WebRequest.CreateHttp(AppConfig.Current.ServerUrl + url);
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

        private static async Task<HttpWebRequest> _CreateRequestWithDataAsync<TData>(string url, string method, TData data)
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

        private static async Task<TResponse> _ExecuteRequest<TResponse>(HttpWebRequest request, bool handleBad)
            where TResponse : OkBadResponse
        {
            string responseBody;
            string errMessage;

            request.CookieContainer = new CookieContainer();
            foreach (var (name, value) in AppConfig.Current.Cookies)
            {
                request.CookieContainer.Add(new Cookie(name, value, null, AppConfig.Current.Domain));
            }
            
            try
            {
                var response = (HttpWebResponse)request.GetResponse();

                await using var stream = response.GetResponseStream();
                using var reader = new StreamReader(stream!);
                
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
                
                Locator.Current.GetService<IDialogService>()!
                    .ShowError(errMessage, more: ReferenceEquals(errMessage, ex.Message) ? null : ex.Message);
                return null;
            }
            
            try
            {
                var data = JsonConvert.DeserializeObject<TResponse>(responseBody);
                if (errMessage is null)
                {
                    if (data is null)
                        throw new FormatException();
                    
                    if (handleBad && data.Failure)
                        Locator.Current.GetService<IDialogService>()!.ShowFailure(
                            data.ToFullLocalizedString(Locator.Current.GetService<IOkBadService>()));
                    
                    return data;
                }

                if (data is null)
                    Locator.Current.GetService<IDialogService>()!.ShowError(errMessage);
                else 
                    Locator.Current.GetService<IDialogService>()!.ShowError(
                        data.ToFullLocalizedString(Locator.Current.GetService<IOkBadService>()));
            }
            catch
            {
                Locator.Current.GetService<IDialogService>()!
                    .ShowError(errMessage ?? Resources.ERR__INVALID_API_RESPONSE, responseBody);
            }

            return null;
        }
    }
}