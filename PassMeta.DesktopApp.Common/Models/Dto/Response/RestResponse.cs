using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PassMeta.DesktopApp.Common.Models.Dto.Response;

/// <summary>
/// PassMeta server unified response.
/// </summary>
public class RestResponse
{
    /// <summary></summary>
    public RestResponse()
    {
        Message ??= string.Empty;
    }

    /// <summary>
    /// Response code.
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; init; }

    /// <summary>
    /// Response message.
    /// </summary>
    [JsonPropertyName("msg")]
    public string Message { get; init; }

    /// <summary>
    /// Additional failure information.
    /// </summary>
    [JsonPropertyName("more")]
    public List<string>? More { get; init; }

    /// <summary>
    /// Is response succeed?
    /// </summary>
    public bool Success => Code == 0;

    /// <summary>
    /// Is response failed?
    /// </summary>
    public bool Failure => Code != 0;
}

/// <summary>
/// <see cref="RestResponse"/> with data.
/// </summary>
public class RestResponse<TData> : RestResponse
{
    /// <summary>
    /// If <see cref="RestResponse.Success"/>, response data.
    /// </summary>
    public TData? Data { get; init; }
}