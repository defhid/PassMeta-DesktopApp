using System;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Dto.Response;

namespace PassMeta.DesktopApp.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="IDialogService"/>
/// </summary>
public static class DialogServiceExtensions
{
    /// <summary>
    /// Show failure message from server response to user.
    /// </summary>
    /// <param name="service">this.</param>
    /// <param name="response">Response.</param>
    /// <param name="title">Message title.</param>
    /// <param name="defaultPresenter">How to show the message. Default is popup, but if not available, dialog window will be used.</param>
    /// <remarks>Auto-logging.</remarks>
    public static void ShowFailure(
        this IDialogService service,
        RestResponse response,
        string? title = null,
        DialogPresenter defaultPresenter = DialogPresenter.Window)
        => service.ShowFailure(
            response.Message,
            title,
            response.More is null ? null : string.Join(Environment.NewLine, response.More), 
            defaultPresenter);
}