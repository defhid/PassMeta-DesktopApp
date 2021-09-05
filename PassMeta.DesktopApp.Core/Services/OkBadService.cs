using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Core.Utils;

namespace PassMeta.DesktopApp.Core.Services
{
    public class OkBadService : IOkBadService
    {
        public string GetLocalizedMessage(string message)
        {
            if (AppConfig.Current.OkBadMessagesTranslatePack.TryGetValue(message, out var dict))
            {
                return dict.TryGetValue(Resources.Culture.Name, out var result) ? result : dict["default"];
            }

            return message;
        }
    }
}