namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models;
    using DesktopApp.Core.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Splat;
    
    /// <inheritdoc />
    public class OkBadService : IOkBadService
    {
        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;
        
        /// <inheritdoc />
        public string GetLocalizedMessage(string message)
        {
            Dictionary<string, string>? dict = null;
            
            // ReSharper disable once ConstantConditionalAccessQualifier
            if (AppConfig.Current?.OkBadMessagesTranslatePack.TryGetValue(message, out dict) is true)
            {
                return dict!.TryGetValue(Resources.Culture.Name, out var result)
                    ? result
                    : dict.TryGetValue("default", out result)
                        ? result
                        : message;
            }

            return message;
        }

        /// <inheritdoc />
        public void ShowResponseFailure(OkBadResponse response, IReadOnlyDictionary<string, string>? whatMapper = null)
        {
            var lines = _ResponseToText(response, whatMapper);
            
            _dialogService.ShowFailure(lines[0], string.Join(Environment.NewLine, lines.Skip(1)));
        }

        private List<string> _ResponseToText(OkBadResponse response, IReadOnlyDictionary<string, string>? whatMapper)
        {
            var message = GetLocalizedMessage(response.Message);
            List<string> builder;
            
            if (response.What is null)
            {
                builder = new List<string> { message };
            }
            else
            {
                string? what = null;
                whatMapper?.TryGetValue(response.What, out what);
                
                builder = new List<string> { message + $": {what ?? response.What}" };
            }

            if (response.More is not null)
                builder.Add(response.More.ToString());

            if (response.Sub is not null)
                builder.AddRange(response.Sub.Select(okBad => "--" + Environment.NewLine + _ResponseToText(okBad, whatMapper)));

            return builder;
        }
    }
}