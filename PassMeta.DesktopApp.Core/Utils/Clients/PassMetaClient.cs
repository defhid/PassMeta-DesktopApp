using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Models.Dto.Response.OkBad;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Core.Services.Extensions;
using PassMeta.DesktopApp.Core.Utils.Json;

namespace PassMeta.DesktopApp.Core.Utils.Clients;

/// <inheritdoc />
public sealed class PassMetaClient : IPassMetaClient
{
    private readonly BehaviorSubject<bool> _onlineSubject = new(false);
    private readonly ILogService _logger;
    private readonly IDialogService _dialogService;
    private readonly IOkBadService _okBadService;
    private readonly HttpClient _httpClient;

    /// <summary></summary>
    public PassMetaClient(
        IAppContextManager appContextManager,
        ILogService logger,
        IDialogService dialogService,
        IOkBadService okBadService)
    {
        _logger = logger;
        _dialogService = dialogService;
        _okBadService = okBadService;
        _httpClient = CreateHttpClient(appContextManager);
    }
        
    /// <inheritdoc />
    public bool Online => _onlineSubject.Value;

    /// <inheritdoc />
    public IObservable<bool> OnlineObservable => _onlineSubject;

    /// <inheritdoc />
    public void Dispose() => _httpClient.Dispose();
        
    /// <inheritdoc />
    public IRequestBuilder Begin(IHttpRequestBase httpRequestBase) 
        => new RequestBuilder(httpRequestBase.Method, httpRequestBase.Url, this);

    /// <inheritdoc />
    public IRequestWithBodySupportBuilder Begin(IHttpRequestWithBodySupportBase httpRequestBase)
        => new RequestBuilder(httpRequestBase.Method, httpRequestBase.Url, this);

    /// <inheritdoc />
    public async Task<bool> CheckConnectionAsync()
    {
        if (AppConfig.Current.ServerUrl is null) return false;

        var has = false;
        try
        {
            var requestBase = PassMetaApi.General.GetCheck();
                
            var request = new HttpRequestMessage(requestBase.Method, AppConfig.Current.ServerUrl + "/" + requestBase.Url);
            var response = await _httpClient.SendAsync(request);

            has = response.StatusCode is HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            if (ex is not HttpRequestException)
            {
                _logger.Error(ex, "Failed to check connection");
            }
        }

        RefreshOnline(has);

        return Online;
    }

    internal async ValueTask<byte[]?> BuildAndExecuteRawAsync(RequestBuilder requestBuilder)
    {
        if (!TryBuildRequestMessage(requestBuilder, out var request))
        {
            return null;
        }

        var context = requestBuilder.Context;

        byte[]? successBody;
        OkBadResponse? failureOkBadResponse;
        try
        {
            (successBody, failureOkBadResponse) = await SendAsync(request, context, content => content.ReadAsByteArrayAsync());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Request failed: {request.GetShortInformation()} [{context}]");
            _dialogService.ShowError(ex.Message, context);
            return null;
        }

        if (successBody is null)
        {
            if (requestBuilder.BadMapper is not null)
                failureOkBadResponse!.More?.ApplyWhatMapping(requestBuilder.BadMapper);

            if (requestBuilder.HandleBad)
                _okBadService.ShowResponseFailure(failureOkBadResponse!, context);

            return null;
        }

        return successBody;
    }

    internal async ValueTask<OkBadResponse<TData>?> BuildAndExecuteAsync<TData>(RequestBuilder requestBuilder)
    {
        if (!TryBuildRequestMessage(requestBuilder, out var request))
        {
            return null;
        }
            
        var context = requestBuilder.Context;
        byte[]? successBody;
        OkBadResponse? failureOkBadResponse;
        OkBadResponse<TData>? successOkBadResponse = null;

        try
        {
            (successBody, failureOkBadResponse) = await SendAsync(request, context, content => content.ReadAsByteArrayAsync());
            if (successBody is not null)
            {
                successOkBadResponse = ParseOkBadResponse<TData>(successBody);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Request failed: {request.GetShortInformation()} [{context}]");
            _dialogService.ShowError(ex.Message, context);
            return null;
        }

        if (requestBuilder.BadMapper is not null)
        {
            failureOkBadResponse?.More?.ApplyWhatMapping(requestBuilder.BadMapper);
            successOkBadResponse?.More?.ApplyWhatMapping(requestBuilder.BadMapper);
        }

        if (successBody is null)
        {
            if (requestBuilder.HandleBad)
                _okBadService.ShowResponseFailure(failureOkBadResponse!, context);

            return ResponseFactory.OkBadWithNullData<TData>(failureOkBadResponse!);
        }

        if (successOkBadResponse!.Success is false)
        {
            _logger.Warning("Failure response with success HTTP code: " + Encoding.UTF8.GetString(successBody));

            if (requestBuilder.HandleBad)
                _okBadService.ShowResponseFailure(successOkBadResponse, context);
        }

        return successOkBadResponse;
    }

    /// <returns>
    /// Success response body and null <see cref="OkBadResponse"/>,
    /// or null response budy and failure <see cref="OkBadResponse"/>.
    /// </returns>
    private async Task<(TBody? SuccessBody, OkBadResponse? FailureResponse)> SendAsync<TBody>(
        HttpRequestMessage message,
        string? context,
        Func<HttpContent, Task<TBody>> handleResponseAsync)
        where TBody : class
    {
        try
        {
            using var response = await _httpClient.SendAsync(message);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                {
                    return (await handleResponseAsync(response.Content), null);
                }
                case HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout:
                {
                    _logger.Error(message.GetShortInformation() + $"{Resources.API__CONNECTION_TIMEOUT_ERR} [{context}]");
                    return (null, ResponseFactory.FakeOkBad(false, Resources.API__CONNECTION_TIMEOUT_ERR));
                }
                default:
                {
                    var responseBody = await response.Content.ReadAsByteArrayAsync();
                    var okBadResponse = ParseOkBadResponse<object>(responseBody);

                    _logger.Warning($"{message.GetShortInformation()} {response.StatusCode} [{context}] {Encoding.UTF8.GetString(responseBody)}");

                    return (null, okBadResponse);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            RefreshOnline(false);
            _logger.Error(ex, $"{message.GetShortInformation()} {Resources.API__CONNECTION_ERR} [{context}]");
            return (null, ResponseFactory.FakeOkBad(false, Resources.API__CONNECTION_ERR));
        }
    }

    private bool TryBuildRequestMessage(
        RequestBuilder requestBuilder,
        [NotNullWhen(true)] out HttpRequestMessage? message)
    {
        if (requestBuilder.Uri is null)
        {
            _dialogService.ShowError(Resources.API__URL_ERR);
            message = null;
            return false;
        }

        try
        {
            message = new HttpRequestMessage(requestBuilder.Method, requestBuilder.Uri);

            if (requestBuilder.Data is not null)
            {
                if (requestBuilder.IsForm) SetFormBody(message, requestBuilder.Data);
                else SetJsonBody(message, requestBuilder.Data);
            }

            return true;
        }
        catch (Exception ex)
        {
            message = null;
            _logger.Error(ex, $"Http request building error (uri: {requestBuilder.Uri})");
            _dialogService.ShowError(ex.Message);
            return false;
        }
    }

    private static void SetJsonBody(HttpRequestMessage message, object data)
    {
        var dataBytes = JsonSerializer.SerializeToUtf8Bytes(data, SerializerOptions);

        message.Content = new ByteArrayContent(dataBytes);
        message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
    }
    
    private static void SetFormBody(HttpRequestMessage message, object data)
    {
        var form = new MultipartFormDataContent($"----{Guid.NewGuid():N}");

        try
        {
            foreach (var property in data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var value = property.GetValue(data);

                if (value is byte[] bytes)
                    form.Add(new ByteArrayContent(bytes), property.Name, "file");
                else
                    form.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(value?.ToString() ?? string.Empty)), property.Name);
            }
        }
        catch
        {
            form.Dispose();
            throw;
        }

        message.Content = form;
    }

    private static OkBadResponse<TData> ParseOkBadResponse<TData>(byte[] body)
    {
        var okBad = JsonSerializer.Deserialize<OkBadResponse<TData>>(body, SerializerOptions);
        return okBad ?? throw new FormatException();
    }

    private void RefreshOnline(bool isOnline)
    {
        if (Online == isOnline)
        {
            return;
        }

        try
        {
            _onlineSubject.OnNext(isOnline);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to set 'Online' state of {nameof(PassMetaClient)}");
        }
    }

    private HttpClient CreateHttpClient(IAppContextManager appContextManager)
    {
        var handler = new PassMetaClientHandler(appContextManager, _logger);
        return new HttpClient(handler, disposeHandler: true);
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
        WriteIndented = false
    };
}