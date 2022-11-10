namespace PassMeta.DesktopApp.Core.Utils
{
    using DesktopApp.Common;
    using DesktopApp.Common.Models;
    
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reactive.Subjects;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Abstractions.Mapping;
    using Common.Abstractions.Services;
    using Common.Abstractions.Utils;
    using Common.Utils.Extensions;
    using Extensions;
    using Newtonsoft.Json;
    using AppContext = Core.AppContext;

    /// <inheritdoc />
    internal class PassMetaClient : IPassMetaClient
    {
        private readonly ILogService _logger;
        private readonly IDialogService _dialogService;
        private readonly IOkBadService _okBadService;

        /// <summary>
        /// Corresponds to the last call of <see cref="CheckConnectionAsync"/>.
        /// </summary>
        public static bool Online => OnlineSubject.Value;

        /// <summary>
        /// Represents <see cref="Online"/>.
        /// </summary>
        public static IObservable<bool> OnlineObservable => OnlineSubject;

        private static readonly BehaviorSubject<bool> OnlineSubject = new(false);

        private static readonly SemaphoreSlim CookiesRefreshSemaphore = new(1, 1);

        static PassMetaClient()
        {
            ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;
        }

        /// <summary></summary>
        public PassMetaClient(ILogService logger, IDialogService dialogService, IOkBadService okBadService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _okBadService = okBadService;
        }

        /// <inheritdoc />
        public IPassMetaClient.IRequestBuilder Get(string url)
            => new RequestBuilder("GET", url, this);

        /// <inheritdoc />
        public IPassMetaClient.IRequestWithBodySupportBuilder Post(string url)
            => new RequestBuilder("POST", url, this);

        /// <inheritdoc />
        public IPassMetaClient.IRequestWithBodySupportBuilder Patch(string url)
            => new RequestBuilder("PATCH", url, this);

        /// <inheritdoc />
        public IPassMetaClient.IRequestWithBodySupportBuilder Delete(string url)
            => new RequestBuilder("DELETE", url, this);

        /// <inheritdoc />
        public async Task<bool> CheckConnectionAsync(bool showNoConnection = false, bool isFromAppContext = false)
        {
            if (AppConfig.Current.ServerUrl is null) return false;

            bool has;
            try
            {
                var request = WebRequest.CreateHttp(AppConfig.Current.ServerUrl + "/check");
                var response = (HttpWebResponse)await request.GetResponseAsync();

                has = response.StatusCode == HttpStatusCode.OK;
            }
            catch (WebException)
            {
                if (showNoConnection)
                    _dialogService.ShowInfo(Resources.API__CONNECTION_ERR);

                has = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Connection checking error");
                has = false;
            }

            if (has && AppContext.Current.ServerVersion is null && !isFromAppContext)
            {
                await AppContext.RefreshCurrentFromServerAsync(this, false);
            }

            if (has != Online) OnlineSubject.OnNext(has);

            return Online;
        }

        internal async ValueTask<TResponse?> BuildAndExecuteAsync<TResponse>(RequestBuilder requestBuilder)
            where TResponse : OkBadResponse
        {
            if (!TryBuildRequest(requestBuilder, out var request))
            {
                return null;
            }
            
            var context = requestBuilder.Context;
            var badMapper = requestBuilder.BadMapper;
            var handleBad = requestBuilder.HandleBad;

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

                if (response.Headers.GetValues("Set-Cookie")?.Any() is not null)
                {
                    await CookiesRefreshSemaphore.WaitAsync();
                    try
                    {
                        var changed = CookiesHelper.RefreshCookies(AppContext.Current.Cookies, response.Cookies);
                        if (changed)
                        {
                            await AppContext.FlushCurrentAsync();
                        }
                    }
                    finally
                    {
                        CookiesRefreshSemaphore.Release();
                    }
                }
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
                    if (Online) OnlineSubject.OnNext(false);
                }
                else if (responseData is not null)
                {
                    if (badMapper is not null)
                        responseData.ApplyWhatMapping(badMapper);

                    _logger.Warning($"{responseBody} [{context}]");

                    if (handleBad)
                        _okBadService.ShowResponseFailure(responseData, context);

                    return responseData;
                }

                errMessage = ex.Status switch
                {
                    WebExceptionStatus.ConnectFailure => Resources.API__CONNECTION_ERR,
                    WebExceptionStatus.Timeout => Resources.API__CONNECTION_TIMEOUT_ERR,
                    _ => ex.Message
                };

                var more = responseBody ?? (ReferenceEquals(errMessage, ex.Message) ? null : ex.Message);

                _logger.Error(ex, request.GetShortInformation() + $" [{context}]");
                _dialogService.ShowError(errMessage, context, request.GetShortInformation().NewLine(more));
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Unknown error, request failed: {request.GetShortInformation()} [{context}]");
                _dialogService.ShowError(ex.Message, context);
                return null;
            }

            try
            {
                responseData = JsonConvert.DeserializeObject<TResponse>(responseBody);
                if (errMessage is null)
                {
                    if (responseData is null)
                        throw new FormatException();

                    if (badMapper is not null)
                        responseData.ApplyWhatMapping(badMapper);

                    if (handleBad && !responseData.Success)
                        _okBadService.ShowResponseFailure(responseData);

                    return responseData;
                }

                _logger.Error($"{errMessage} {request.GetShortInformation()} {responseBody} [{context}]");
                _dialogService.ShowError(errMessage, context, request.GetShortInformation().NewLine(responseBody));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{request.GetShortInformation()} {responseBody} [{context}]");
                _dialogService.ShowError(errMessage ?? Resources.API__INVALID_RESPONSE_ERR, context, responseBody);
            }

            return null;
        }

        private bool TryBuildRequest(RequestBuilder requestBuilder, [NotNullWhen(true)] out HttpWebRequest? request)
        {
            if (requestBuilder.Uri is null)
            {
                _dialogService.ShowError(Resources.API__URL_ERR);
                request = null;
                return false;
            }
            
            try
            {
                request = WebRequest.CreateHttp(requestBuilder.Uri);
                request.Method = requestBuilder.Method;

                if (requestBuilder.Data is not null)
                {
                    if (requestBuilder.IsForm) SetFormBody(request, requestBuilder.Data);
                    else SetJsonBody(request, requestBuilder.Data);
                }

                return true;
            }
            catch (Exception ex)
            {
                request = null;
                _logger.Error(ex, $"Http request building error (uri: {requestBuilder.Uri})");
                _dialogService.ShowError(ex.Message);
                return false;
            }
        }

        private static void SetJsonBody(WebRequest request, object data)
        {
            var dataBytes = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(data));

            using var dataStream = request.GetRequestStream();
            dataStream.Write(dataBytes, 0, dataBytes.Length);

            request.ContentType = "application/json";
            request.ContentLength = dataStream.Length;
        }
    
        private static void SetFormBody(WebRequest request, object data)
        {
            using var form = new MultipartFormDataContent();

            foreach (var property in data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var value = property.GetValue(data);
                form.Add(value is byte[] bytes
                    ? new ByteArrayContent(bytes)
                    : new StringContent(value?.ToString() ?? string.Empty), property.Name);
            }

            using var dataStream = request.GetRequestStream();
            form.CopyTo(dataStream, null, CancellationToken.None);

            request.ContentType = "multipart/form-data";
            request.ContentLength = dataStream.Length;
        }
    }
    
    /// <inheritdoc />
    internal class RequestBuilder : IPassMetaClient.IRequestWithBodySupportBuilder
    {
        private readonly PassMetaClient _client;

        internal readonly Uri? Uri;

        internal readonly string Method;

        internal object? Data { get; private set; }

        internal bool IsForm { get; private set; }

        internal string? Context { get; private set; }

        internal IMapper<string, string>? BadMapper { get; private set; }

        internal bool HandleBad { get; private set; }

        /// <summary></summary>
        public RequestBuilder(string method, string url, PassMetaClient client)
        {
            _client = client;
            Uri = BuildUri(url);
            Method = method;
            Context = null;
            BadMapper = null;
            HandleBad = false;
        }
        
        /// <inheritdoc />
        public IPassMetaClient.IRequestBuilder WithBadMapping(IMapper<string, string> whatMapper)
        {
            if (BadMapper is null)
                BadMapper = whatMapper;
            else
                BadMapper += whatMapper;
            return this;
        }
        
        /// <inheritdoc />
        public IPassMetaClient.IRequestBuilder WithBadHandling()
        {
            HandleBad = true;
            return this;
        }
        
        /// <inheritdoc />
        public IPassMetaClient.IRequestBuilder WithContext(string? context)
        {
            Context = string.IsNullOrWhiteSpace(context) ? null : context;
            return this;
        }
        
        /// <inheritdoc />
        public IPassMetaClient.IRequestWithBodySupportBuilder WithJsonBody(object data)
        {
            Data = data;
            IsForm = false;
            return this;
        }

        /// <inheritdoc />
        public IPassMetaClient.IRequestWithBodySupportBuilder WithFormBody(object data)
        {
            Data = data;
            IsForm = true;
            return this;
        }

        /// <inheritdoc />
        public async ValueTask<OkBadResponse?> ExecuteAsync()
            => await _client.BuildAndExecuteAsync<OkBadResponse<object>>(this);

        /// <inheritdoc />
        public async ValueTask<OkBadResponse<TResponseData>?> ExecuteAsync<TResponseData>()
            => await _client.BuildAndExecuteAsync<OkBadResponse<TResponseData>>(this);

        private static Uri? BuildUri(string url)
        {
            url = AppConfig.Current.ServerUrl + '/' + url +
                  (url.Contains('?') ? '&' : '?') +
                  "lang=" + AppConfig.Current.CultureCode;
            try
            {
                return new Uri(url);
            }
            catch
            {
                return null;
            }
        }
    }
}