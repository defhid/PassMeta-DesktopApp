namespace PassMeta.DesktopApp.Common.Models.Dto.Response.OkBad;

/// <summary>
/// PassMeta server unified response.
/// </summary>
public class OkBadResponse
{
    /// <summary></summary>
    public OkBadResponse()
    {
        Msg ??= string.Empty;
    }

    /// <summary>
    /// Response code.
    /// </summary>
    public int Code { get; init; }

    /// <summary>
    /// Response message.
    /// </summary>
    public string Msg { get; init; }

    /// <summary>
    /// Additional failure information.
    /// </summary>
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
    public TData? Data { get; init; }
}