using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Dto.Response.OkBad;

/// <summary>
/// PassMeta server unified response.
/// </summary>
public class OkBadResponse
{
    /// <summary>
    /// Response code.
    /// </summary>
    [JsonProperty("code")]
    public int Code { get; init; }

    /// <summary>
    /// Response message.
    /// </summary>
    [JsonProperty("msg")]
    public string Message { get; init; } = null!;

    /// <summary>
    /// Additional failure information.
    /// </summary>
    [JsonProperty("more")]
    public OkBadMore? More { get; init; }

    /// <summary>
    /// Is response success?
    /// </summary>
    public bool Success => Code == 0;
}

/// <summary>
/// <see cref="OkBadResponse"/> with data.
/// </summary>
public class OkBadResponse<TData> : OkBadResponse
{
    /// <summary>
    /// If <see cref="OkBadResponse.Success"/>, response data.
    /// </summary>
    [JsonProperty("data")]
    public TData? Data { get; init; }
}