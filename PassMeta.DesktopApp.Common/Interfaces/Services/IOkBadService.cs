using System.Collections.Generic;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models;

namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    public interface IOkBadService
    {
        string? GetLocalizedMessage(string message);

        Task ShowResponseFailureAsync(OkBadResponse response, IDictionary<string, string>? whatMapper = null);
    }
}