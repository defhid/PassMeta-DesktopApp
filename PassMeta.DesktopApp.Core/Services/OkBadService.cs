using System.Collections.Generic;
using PassMeta.DesktopApp.Common.Interfaces.Services;

namespace PassMeta.DesktopApp.Core.Services
{
    public class OkBadService : IOkBadService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _messagesTranslatePack;
        
        public string GetLocalizedMessage(string message)
        {
            if (_messagesTranslatePack.TryGetValue(message, out var dict))
            {
                return dict.TryGetValue(Resources.Culture.Name, out var result) ? result : dict["default"];
            }

            return message;
        }

        public OkBadService(Dictionary<string, Dictionary<string, string>> messagesTranslatePack)
        {
            _messagesTranslatePack = messagesTranslatePack ?? new Dictionary<string, Dictionary<string, string>>();
        }
    }
}