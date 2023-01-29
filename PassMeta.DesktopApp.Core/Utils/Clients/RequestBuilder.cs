using System;
using System.Net.Http;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;
using PassMeta.DesktopApp.Common.Models.Dto.Response.OkBad;

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

    internal IValuesMapper<string, string>? BadMapper { get; private set; }

    internal bool HandleBad { get; private set; }

    /// <summary></summary>
    public RequestBuilder(HttpMethod method, string url, PassMetaClient client)
    {
        _client = client;
        Uri = BuildUri(url);
        Method = method;
        Context = null;
        BadMapper = null;
        HandleBad = false;
    }
        
    /// <inheritdoc />
    public IRequestBuilder WithBadMapping(IValuesMapper<string, string> whatValuesMapper)
    {
        if (BadMapper is null)
            BadMapper = whatValuesMapper;
        else
            BadMapper += whatValuesMapper;
        return this;
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
    public async ValueTask<OkBadResponse?> ExecuteAsync()
        => await _client.BuildAndExecuteAsync<object>(this);

    /// <inheritdoc />
    public async ValueTask<OkBadResponse<TResponseData>?> ExecuteAsync<TResponseData>()
        => await _client.BuildAndExecuteAsync<TResponseData>(this);

    /// <inheritdoc />
    public async ValueTask<byte[]?> ExecuteRawAsync()
        => await _client.BuildAndExecuteRawAsync(this);

    private static Uri? BuildUri(string url)
    {
        url = AppConfig.Current.ServerUrl + '/' + url +
              (url.Contains('?') ? '&' : '?') +
              "lang=" + AppConfig.Current.Culture.Code;
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