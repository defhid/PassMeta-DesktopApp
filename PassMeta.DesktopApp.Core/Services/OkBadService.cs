using System;
using System.Collections.Generic;
using System.Linq;

using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Utils.Extensions;

namespace PassMeta.DesktopApp.Core.Services;

/// <inheritdoc />
public class OkBadService : IOkBadService
{
    private readonly IDialogService _dialogService;

    /// <summary></summary>
    public OkBadService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    /// <inheritdoc />
    public void ShowResponseFailure(OkBadResponse response, string? context = null)
    {
        var lines = _ResponseToText(response);
            
        _dialogService.ShowFailure(lines[0], context, string.Join(Environment.NewLine, lines.Skip(1)));
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