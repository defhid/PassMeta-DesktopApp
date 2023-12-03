using PassMeta.DesktopApp.Common.Models.Dto.Response;

namespace PassMeta.DesktopApp.Core.Utils.Clients;

internal static class RestResponseFactory
{
    public static RestResponse Ok() => new() { Code = 0 };

    public static RestResponse<TData> Ok<TData>(TData data) => new() { Code = 0, Data = data };

    public static RestResponse Bad(string message) => new() { Code = -1, Message = message };

    public static RestResponse<TData> Bad<TData>(string message) => new() { Code = -1, Message = message };

    public static RestResponse<TData> WithNullData<TData>(RestResponse origin) => new()
    {
        Code = origin.Code,
        Message = origin.Message,
        More = origin.More,
    };
}