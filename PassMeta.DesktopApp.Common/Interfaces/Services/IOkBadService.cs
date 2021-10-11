using System.Collections.Generic;
using PassMeta.DesktopApp.Common.Models;

namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    public interface IOkBadService
    {
        string? GetLocalizedMessage(string message);

        void ShowResponseFailure(OkBadResponse response, IDictionary<string, string>? whatMapper = null);
    }
}