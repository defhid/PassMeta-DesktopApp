using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Models.Dto.Response;

namespace PassMeta.DesktopApp.Core.Utils.Clients;

/// <inheritdoc />
internal class RequestBuilder : IRequestWithBodySupportBuilder
{
    private readonly PassMetaClient _client;

    internal readonly Uri? Uri;

    internal readonly HttpMethod Method;

    internal object? Data { get; private set; }

    internal bool IsForm { get; private set; }

    internal string? Context { get; private set; }

    internal bool HandleBad { get; private set; }

    /// <summary></summary>
    public RequestBuilder(HttpMethod method, string url, PassMetaClient client)
    {
        _client = client;
        Uri = BuildUri(url);
        Method = method;
        Context = null;
        HandleBad = false;
    }

    /// <inheritdoc />
    public IRequestBuilder WithBadHandling()
    {
        HandleBad = true;
        return this;
    }
        
    /// <inheritdoc />
    public IRequestBuilder WithContext(string? context)
    {
        Context = string.IsNullOrWhiteSpace(context) ? null : context;
        return this;
    }

    /// <inheritdoc />
    public IRequestWithBodySupportBuilder WithJsonBody(object data)
    {
        Data = data;
        IsForm = false;
        return this;
    }

    /// <inheritdoc />
    public IRequestWithBodySupportBuilder WithFormBody(object data)
    {
        Data = data;
        IsForm = true;
        return this;
    }

    /// <inheritdoc />
    public async ValueTask<RestResponse> ExecuteAsync(CancellationToken cancellationToken = default)
        => await Task.Run(
            () => _client.BuildAndExecuteAsync(this, cancellationToken),
            cancellationToken);

    /// <inheritdoc />
    public async ValueTask<RestResponse<TResponseData>> ExecuteAsync<TResponseData>(CancellationToken cancellationToken = default)
        where TResponseData : class
        => await Task.Run(
            () => _client.BuildAndExecuteAsync<TResponseData>(this, cancellationToken),
            cancellationToken);

    private Uri? BuildUri(string url)
    {
        url = _client.AppConfigProvider.Current.ServerUrl + '/' + url +
              (url.Contains('?') ? '&' : '?') +
              "lang=" + _client.AppConfigProvider.Current.Culture.Code;
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