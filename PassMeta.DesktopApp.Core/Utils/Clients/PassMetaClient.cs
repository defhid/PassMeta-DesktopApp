using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Dto.Response;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Core.Utils.Json;

namespace PassMeta.DesktopApp.Core.Utils.Clients;

/// <inheritdoc />
public sealed class PassMetaClient : IPassMetaClient
{
    private readonly BehaviorSubject<bool> _onlineSubject = new(false);
    private readonly ILogsWriter _logger;
    private readonly IDialogService _dialogService;
    private readonly HttpClient _httpClient;

    internal readonly IAppConfigProvider AppConfigProvider;

    static PassMetaClient()
        => ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;

    /// <summary></summary>
    public PassMetaClient(
        IAppContextManager appContextManager,
        IAppConfigProvider appConfigProvider,
        ILogsWriter logger,
        IDialogService dialogService)
    {
        AppConfigProvider = appConfigProvider;
        _logger = logger;
        _dialogService = dialogService;
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
    public async Task<bool> CheckConnectionAsync(bool reset = false)
    {
        if (AppConfigProvider.Current.ServerUrl is null)
        {
            SetOnline(false);
            return false;
        }

        if (reset)
        {
            SetOnline(false);
        }

        var has = false;
        try
        {
            var requestBase = PassMetaApi.General.GetCheck();
                
            var request = new HttpRequestMessage(requestBase.Method, AppConfigProvider.Current.ServerUrl + "/" + requestBase.Url);
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

        SetOnline(has);

        return Online;
    }

    internal async Task<RestResponse> BuildAndExecuteAsync(
        RequestBuilder requestBuilder,
        CancellationToken cancellationToken)
    {
        if (!TryBuildRequestMessage(requestBuilder, out var request))
        {
            return RestResponseFactory.Bad("Failed to build request");
        }
            
        var context = requestBuilder.Context;
        RestResponse? failureRestResponse;

        try
        {
            (_, failureRestResponse) = await SendAsync(
                request, context, content => content.ReadAsByteArrayAsync(cancellationToken), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Request failed: {request.GetShortInformation()} [{context}]");
            _dialogService.ShowError(ex.Message, context);
            return RestResponseFactory.Bad("Failed to execute request");
        }

        if (failureRestResponse is null)
        {
            return RestResponseFactory.Ok();
        }

        if (requestBuilder.HandleBad)
            _dialogService.ShowFailure(failureRestResponse, context);

        return failureRestResponse;
    }
    
    internal async Task<RestResponse<TData>> BuildAndExecuteAsync<TData>(
        RequestBuilder requestBuilder,
        CancellationToken cancellationToken)
        where TData : class
    {
        if (!TryBuildRequestMessage(requestBuilder, out var request))
        {
            return RestResponseFactory.Bad<TData>("Failed to build request");
        }
            
        var context = requestBuilder.Context;
        RestResponse? failureRestResponse;
        TData? successPayload = null;

        try
        {
            (var successBody, failureRestResponse) = await SendAsync(
                request, context, content => content.ReadAsByteArrayAsync(cancellationToken), cancellationToken);

            if (successBody is not null)
            {
                if (typeof(TData) == typeof(byte[]))
                {
                    successPayload = successBody as TData;
                }
                else
                {
                    successPayload = JsonSerializer.Deserialize<TData>(successBody, SerializerOptions);
                }

                if (successPayload is null)
                {
                    throw new FormatException("Success response has no valid body");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Request failed: {request.GetShortInformation()} [{context}]");
            _dialogService.ShowError(ex.Message, context);
            return RestResponseFactory.Bad<TData>("Failed to execute request");
        }

        if (successPayload is not null)
        {
            return RestResponseFactory.Ok(successPayload);
        }
        
        if (requestBuilder.HandleBad)
            _dialogService.ShowFailure(failureRestResponse!, context);

        return RestResponseFactory.WithNullData<TData>(failureRestResponse!);
    }

    /// <returns>
    /// Success response body and null <see cref="RestResponse"/>,
    /// or null response budy and failure <see cref="RestResponse"/>.
    /// </returns>
    private async Task<(TBody? SuccessBody, RestResponse? FailureResponse)> SendAsync<TBody>(
        HttpRequestMessage message,
        string? context,
        Func<HttpContent, Task<TBody>> handleResponseAsync,
        CancellationToken cancellationToken)
        where TBody : class
    {
        try
        {
            using var response = await _httpClient.SendAsync(message, cancellationToken);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                {
                    SetOnline(true);
                    return (await handleResponseAsync(response.Content), null);
                }
                case HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout:
                {
                    SetOnline(false);
                    _logger.Error(message.GetShortInformation() + $"{Resources.API__CONNECTION_TIMEOUT_ERR} [{context}]");
                    return (null, RestResponseFactory.Bad(Resources.API__CONNECTION_TIMEOUT_ERR));
                }
                default:
                {
                    var responseBody = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                    var restResponse = JsonSerializer.Deserialize<RestResponse>(responseBody, SerializerOptions);
                    if (restResponse is null)
                    {
                        throw new FormatException("Failure response has no valid body");
                    }

                    SetOnline(true);
                    _logger.Warning($"{message.GetShortInformation()} {response.StatusCode} [{context}] {Encoding.UTF8.GetString(responseBody)}");

                    return (null, restResponse);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            SetOnline(false);
            _logger.Error(ex, $"{message.GetShortInformation()} {Resources.API__CONNECTION_ERR} [{context}]");
            return (null, RestResponseFactory.Bad(Resources.API__CONNECTION_ERR));
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

    private void SetOnline(bool isOnline)
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