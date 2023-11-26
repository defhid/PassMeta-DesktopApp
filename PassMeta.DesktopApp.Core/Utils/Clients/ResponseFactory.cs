using PassMeta.DesktopApp.Common.Models.Dto.Response.OkBad;

namespace PassMeta.DesktopApp.Core.Utils.Clients;

internal static class ResponseFactory
{
    public static OkBadResponse FakeOkBad(bool success, string message) => new()
    {
        Code = success ? 0 : -1,
        Msg = message
    };
        
    public static OkBadResponse<TData> OkBadWithNullData<TData>(OkBadResponse origin) => new()
    {
        Code = origin.Code,
        Msg = origin.Msg,
        More = origin.More,
    };
}