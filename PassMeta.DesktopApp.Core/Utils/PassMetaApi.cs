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
            var request = await _CreateRequestAsync(url);
            if (request is null) return null;
            
            request.Method = "GET";
            
            return await _ExecuteRequest<OkBadResponse<TData>>(request, handleBad);
        }
        
        public static async Task<OkBadResponse<TResponseData>> PostAsync<TPostData, TResponseData>(string url, 
            TPostData data, bool handleBad = false)
        {
            var request = await _CreateRequestWithDataAsync(url, data);
            if (request is null) return null;
            
            request.Method = "POST";

            return await _ExecuteRequest<OkBadResponse<TResponseData>>(request, handleBad);
        }
        
        public static async Task<OkBadResponse<TResponseData>> PatchAsync<TPatchData, TResponseData>(string url, 
            TPatchData data, bool handleBad = false)
        {
            var request = await _CreateRequestWithDataAsync(url, data);
            if (request is null) return null;
            
            request.Method = "PATCH";

            return await _ExecuteRequest<OkBadResponse<TResponseData>>(request, handleBad);
        }

        private static async Task<HttpWebRequest> _CreateRequestAsync(string url)
        {
            try
            {
                return WebRequest.CreateHttp(AppConfig.Current.ServerUrl + url);
            }
            catch (UriFormatException)
            {
                await Locator.Current.GetService<IDialogService>()
                    .ShowErrorAsync(Resources.ERR_URL);
                return null;
            }
            catch (Exception ex)
            {
                await Locator.Current.GetService<IDialogService>().ShowErrorAsync(ex.Message);
                return null;
            }
        }

        private static async Task<HttpWebRequest> _CreateRequestWithDataAsync<TData>(string url, TData data)
        {
            try
            {
                var request = WebRequest.CreateHttp(AppConfig.Current.ServerUrl + url);
                var dataBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
                
                request.ContentType = "application/json";
                request.ContentLength = dataBytes.Length;

                await using var dataStream = request.GetRequestStream();
                await dataStream.WriteAsync(dataBytes.AsMemory(0, dataBytes.Length));

                return request;
            }
            catch (Exception ex)
            {
                await Locator.Current.GetService<IDialogService>().ShowErrorAsync(ex.Message);
                return null;
            }
        }

        private static async Task<TResponse> _ExecuteRequest<TResponse>(HttpWebRequest request, bool handleBad)
            where TResponse : OkBadResponse
        {
            string responseBody;
            string errMessage;
            
            try
            {
                using var response = (HttpWebResponse) await request.GetResponseAsync();

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
            }
            catch (WebException ex)
            {
                errMessage = ex.Status switch
                {
                    WebExceptionStatus.ConnectFailure => Resources.ERR__CONNECTION_FAILURE,
                    WebExceptionStatus.Timeout => Resources.ERR__CONNECTION_TIMEOUT,
                    _ => ex.Message
                };
                
                await Locator.Current.GetService<IDialogService>()
                    .ShowErrorAsync(errMessage, more: ReferenceEquals(errMessage, ex.Message) ? null : ex.Message);
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
                        await Locator.Current.GetService<IDialogService>().ShowFailureAsync(
                            data.ToFullLocalizedString(Locator.Current.GetService<IOkBadService>()));
                    
                    return data;
                }

                if (data is null)
                    await Locator.Current.GetService<IDialogService>().ShowErrorAsync(errMessage);
                else 
                    await Locator.Current.GetService<IDialogService>().ShowErrorAsync(
                        data.ToFullLocalizedString(Locator.Current.GetService<IOkBadService>()));
            }
            catch
            {
                if (errMessage is not null)
                    await Locator.Current.GetService<IDialogService>().ShowErrorAsync(errMessage, more: responseBody);
                else 
                    await Locator.Current.GetService<IDialogService>()
                        .ShowErrorAsync(Resources.ERR__INVALID_API_RESPONSE, more: responseBody);
            }

            return null;
        }
    }
}