using PassMeta.DesktopApp.Common.Models;

namespace PassMeta.DesktopApp.Core.Utils.Clients;

internal static class ResponseFactory
{
    public static OkBadResponse FakeOkBad(bool success, string message) => new()
    {
        Code = success ? 0 : -1,
        Message = message
    };
        
    public static OkBadResponse<TData> OkBadWithNullData<TData>(OkBadResponse origin) => new()
    {
        Code = origin.Code,
        Message = origin.Message,
        What = origin.What,
        More = origin.More,
        Sub = origin.Sub
    };
}