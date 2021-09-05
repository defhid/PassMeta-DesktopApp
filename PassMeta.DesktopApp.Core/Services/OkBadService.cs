using System;
using System.Collections.Generic;
using System.Linq;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Core.Utils;
using Splat;

namespace PassMeta.DesktopApp.Core.Services
{
    public class OkBadService : IOkBadService
    {
        public string GetLocalizedMessage(string message)
        {
            Dictionary<string, string> dict = null;
            
            if (AppConfig.Current?.OkBadMessagesTranslatePack.TryGetValue(message, out dict) is true)
            {
                return dict.TryGetValue(Resources.Culture.Name, out var result) ? result : dict["default"];
            }

            return message;
        }

        public void ShowResponseFailure(OkBadResponse response, IDictionary<string, string> whatMapper = null)
        {
            var lines = _ResponseToText(response, whatMapper);
            
            Locator.Current.GetService<IDialogService>()!
                .ShowFailure(lines[0], string.Join(Environment.NewLine, lines.Skip(1)));
        }

        private List<string> _ResponseToText(OkBadResponse response, IDictionary<string, string> whatMapper)
        {
            var message = GetLocalizedMessage(response.Message) ?? response.Message;
            List<string> builder;
            
            if (response.What is null)
            {
                builder = new List<string> { message };
            }
            else
            {
                var what = response.What;
                whatMapper?.TryGetValue(what, out what);
                builder = new List<string> { message + $": {what}" };
            }

            if (response.More is not null)
                builder.Add(response.More.ToString());

            if (response.Sub is not null)
                builder.AddRange(response.Sub.Select(okBad => "--" + Environment.NewLine + _ResponseToText(okBad, whatMapper)));

            return builder;
        }
    }
}