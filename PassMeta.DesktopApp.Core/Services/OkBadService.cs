namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Interfaces.Services;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Utils.Extensions;
    using Splat;

    /// <inheritdoc />
    public class OkBadService : IOkBadService
    {
        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;

        /// <inheritdoc />
        public void ShowResponseFailure(OkBadResponse response)
        {
            var lines = _ResponseToText(response);
            
            _dialogService.ShowFailure(lines[0], string.Join(Environment.NewLine, lines.Skip(1)));
        }

        private static List<string> _ResponseToText(OkBadResponse response, int level = 0)
        {
            var message = response.Message.Capitalize();

            var builder = response.What is null
                ? new List<string> { message }
                : new List<string> { $"{message}: {response.What}" };

            if (response.More is not null)
            {
                builder.Add(response.More.ToString());
            }

            if (response.Sub?.Count > 0)
            {
                level += 2;
                builder.AddRange(response.Sub.Select(okBad => "* ".PadRight(level) + string.Join(
                    Environment.NewLine + string.Concat(Enumerable.Repeat(' ', level + 1)), 
                    _ResponseToText(okBad, level))));
            }

            return builder;
        }
    }
}