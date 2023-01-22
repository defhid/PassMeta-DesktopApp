using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Enums;

namespace PassMeta.DesktopApp.Common.Abstractions.Services;

/// <summary>
/// Service for communication with app user.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Show information message to user.
    /// </summary>
    /// <param name="message">Primary content.</param>
    /// <param name="title">Message title.</param>
    /// <param name="more">Secondary content.</param>
    /// <param name="defaultPresenter">How to show the message. Default is popup, but if not available, dialog window will be used.</param>
    void ShowInfo(string message, string? title = null, string? more = null, DialogPresenter defaultPresenter = DialogPresenter.PopUp);

    /// <summary>
    /// Show error message to user.
    /// </summary>
    /// <param name="message">Primary content.</param>
    /// <param name="title">Message title.</param>
    /// <param name="more">Secondary content.</param>
    /// <param name="defaultPresenter">How to show the message. Default is popup, but if not available, dialog window will be used.</param>
    /// <remarks>Auto-logging.</remarks>
    void ShowError(string message, string? title = null, string? more = null, DialogPresenter defaultPresenter = DialogPresenter.PopUp);
        
    /// <summary>
    /// Show failure message to user.
    /// </summary>
    /// <param name="message">Primary content.</param>
    /// <param name="title">Message title.</param>
    /// <param name="more">Secondary content.</param>
    /// <param name="defaultPresenter">How to show the message. Default is popup, but if not available, dialog window will be used.</param>
    /// <remarks>Auto-logging.</remarks>
    void ShowFailure(string message, string? title = null, string? more = null, DialogPresenter defaultPresenter = DialogPresenter.Window);

    /// <summary>
    /// Ask user for confirmation.
    /// </summary>
    /// <param name="message">Confirmation content.</param>
    /// <param name="title">Window title.</param>
    /// <returns>Did user answered YES?</returns>
    Task<IResult> ConfirmAsync(string message, string? title = null);

    /// <summary>
    /// Ask user for string value.
    /// </summary>
    /// <param name="message">Question content.</param>
    /// <param name="title">Window title.</param>
    /// <param name="defaultValue">The value filled in the textbox by default.</param>
    /// <returns>
    /// Result with not null trimmed string value.
    /// </returns>
    Task<IResult<string>> AskStringAsync(string message, string? title = null, string? defaultValue = null);
        
    /// <summary>
    /// Ask user for string password.
    /// </summary>
    /// <param name="message">Question content.</param>
    /// <param name="title">Window title.</param>
    /// <returns>
    /// Result with not null raw password.
    /// </returns>
    Task<IResult<string>> AskPasswordAsync(string message, string? title = null);

    /// <summary>
    /// Show all deferred messages.
    /// </summary>
    void Flush();
}