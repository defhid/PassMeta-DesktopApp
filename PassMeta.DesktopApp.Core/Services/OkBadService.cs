namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common;
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Interfaces.Mapping;
    using DesktopApp.Common.Interfaces.Services;
    using Utils.Extensions;
    
    using AppContext = Utils.AppContext;

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
            if (AppContext.Current?.OkBadMessagesTranslatePack.TryGetValue(message, out dict) is true)
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
        public void ShowResponseFailure(OkBadResponse response, IMapper? whatMapper = null)
        {
            var lines = _ResponseToText(response, whatMapper);
            
            _dialogService.ShowFailure(lines[0], string.Join(Environment.NewLine, lines.Skip(1)));
        }

        private List<string> _ResponseToText(OkBadResponse response, IMapper? whatMapper, int level = 0)
        {
            var message = GetLocalizedMessage(response.Message).Capitalize();

            var builder = response.What is null
                ? new List<string> { message }
                : new List<string> { message + $": {whatMapper?.Map(response.What) ?? response.What}" };

            if (response.More is not null)
            {
                builder.Add(response.More.ToString());
            }

            if (response.Sub?.Count > 0)
            {
                level += 2;
                builder.AddRange(response.Sub.Select(okBad => "* ".PadRight(level) + string.Join(
                    Environment.NewLine + string.Concat(Enumerable.Repeat(' ', level + 1)), 
                    _ResponseToText(okBad, whatMapper, level))));
            }

            return builder;
        }
    }
}