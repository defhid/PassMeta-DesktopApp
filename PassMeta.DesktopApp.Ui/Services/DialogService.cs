using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.DialogWin;
using PassMeta.DesktopApp.Ui.Views.Windows;

namespace PassMeta.DesktopApp.Ui.Services;

/// <inheritdoc />
public class DialogService : IDialogService
{
    private readonly Func<INotificationManager?> _notificationManagerResolver;
    private readonly Func<IHostWindowProvider?> _hostWindowProviderResolver;
    private readonly ILogsWriter _logger;
    private readonly List<Action> _deferred = new();
    private readonly List<Window> _opened = new();

    public DialogService(
        Func<INotificationManager?> notificationManagerResolver,
        Func<IHostWindowProvider?> hostWindowProviderResolver,
        ILogsWriter logger)
    {
        _notificationManagerResolver = notificationManagerResolver;
        _hostWindowProviderResolver = hostWindowProviderResolver;
        _logger = logger;
    }

    private INotificationManager? NotificationManager => _notificationManagerResolver();

    private Window? HostWindow => _hostWindowProviderResolver()?.Window;

    /// <inheritdoc />
    public void Flush()
    {
        if (HostWindow is null)
        {
            return;
        }

        lock (_deferred)
        {
            foreach (var shower in _deferred)
            {
                try
                {
                    shower();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Deferred dialog showing failed");
                }
            }

            _deferred.Clear();
        }
    }

    /// <inheritdoc />
    public void ShowInfo(string message, string? title = null, string? more = null,
        DialogPresenter defaultPresenter = DialogPresenter.PopUp)
    {
        CallOrDeffer(() =>
        {
            if (defaultPresenter is DialogPresenter.PopUp && NotificationManager is not null)
            {
                message += more is null ? string.Empty : Environment.NewLine + $"[{more}]";
                
                ShowNotification(new Notification(
                    title ?? message,
                    title is null ? null : message,
                    NotificationType.Information, TimeSpan.FromSeconds(2.5)));
            }
            else
            {
                ShowDialog(new DialogWinModel(
                    title ?? Resources.DIALOG__DEFAULT_INFO_TITLE,
                    message,
                    more,
                    new[] { DialogButton.Close },
                    DialogWindowIcon.Info,
                    null));
            }
        });
    }

    public void ShowWarning(string message, string? title = null, string? more = null,
        DialogPresenter defaultPresenter = DialogPresenter.PopUp)
    {
        CallOrDeffer(() =>
        {
            if (defaultPresenter is DialogPresenter.PopUp && NotificationManager is not null)
            {
                message += more is null ? string.Empty : Environment.NewLine + $"[{more}]";
                
                ShowNotification(new Notification(
                    title ?? message,
                    title is null ? null : message,
                    NotificationType.Warning, TimeSpan.FromSeconds(2.5)));
            }
            else
            {
                ShowDialog(new DialogWinModel(
                    title ?? Resources.DIALOG__DEFAULT_WARNING_TITLE,
                    message,
                    more,
                    new[] { DialogButton.Close },
                    DialogWindowIcon.Failure,
                    null));
            }
        });
    }

    /// <inheritdoc />
    public void ShowError(string message, string? title = null, string? more = null,
        DialogPresenter defaultPresenter = DialogPresenter.PopUp)
    {
        CallOrDeffer(() =>
        {
            if (defaultPresenter is DialogPresenter.PopUp && NotificationManager is not null)
            {
                ShowNotification(new Notification(
                    title ?? Resources.DIALOG__DEFAULT_ERROR_TITLE,
                    message + (more is null ? string.Empty : Environment.NewLine + $"[{more}]"),
                    NotificationType.Error,
                    TimeSpan.FromSeconds(16)));
            }
            else
            {
                ShowDialog(new DialogWinModel(
                    title ?? Resources.DIALOG__DEFAULT_ERROR_TITLE,
                    message,
                    more,
                    new[] { DialogButton.Close },
                    DialogWindowIcon.Error,
                    null));
            }
        });
    }

    /// <inheritdoc />
    public void ShowFailure(string message, string? title = null, string? more = null,
        DialogPresenter defaultPresenter = DialogPresenter.Window)
    {
        CallOrDeffer(() =>
        {
            if (defaultPresenter is DialogPresenter.PopUp && NotificationManager is not null)
            {
                ShowNotification(new Notification(
                    title ?? Resources.DIALOG__DEFAULT_FAILURE_TITLE,
                    message + (more is null ? string.Empty : Environment.NewLine + $"[{more}]"),
                    NotificationType.Warning,
                    TimeSpan.FromSeconds(8)));
            }
            else
            {
                ShowDialog(new DialogWinModel(
                    title ?? Resources.DIALOG__DEFAULT_FAILURE_TITLE,
                    message,
                    more,
                    new[] { DialogButton.Close },
                    DialogWindowIcon.Failure,
                    null));
            }
        });
    }

    /// <inheritdoc />
    /// <remarks>Use only when the main window has been rendered.</remarks>
    public async Task<IResult> ConfirmAsync(string message, string? title = null)
    {
        if (HostWindow is null) return Result.Failure();

        var dialog = await ShowDialogAndWaitAsync(new DialogWinModel(
            title ?? Resources.DIALOG__DEFAULT_CONFIRM_TITLE,
            message,
            null,
            new[] { DialogButton.Yes, DialogButton.Cancel },
            DialogWindowIcon.Confirm,
            null));

        return Result.From(dialog.ViewModel!.Result is DialogButton.Yes);
    }

    /// <inheritdoc />
    /// <remarks>Use only when the main window has been rendered.</remarks>
    public async Task<IResult<string>> AskStringAsync(string message, string? title = null, string? defaultValue = null)
    {
        if (HostWindow is null) return Result.Failure<string>();

        var dialog = await ShowDialogAndWaitAsync(new DialogWinModel(
            title ?? Resources.DIALOG__DEFAULT_ASK_TITLE,
            message,
            null,
            new[] { DialogButton.Ok, DialogButton.Cancel },
            null,
            new TextInputBox(true, "", defaultValue, null)));

        var value = ((DialogWinModel)dialog.DataContext!).TextInputBox.Value?.Trim();

        return Result.From(dialog.ViewModel!.Result is DialogButton.Ok, value ?? string.Empty);
    }

    /// <inheritdoc />
    /// <remarks>Use only when the main window has been rendered.</remarks>
    public async Task<IResult<string>> AskPasswordAsync(string message, string? title = null)
    {
        if (HostWindow is null) return Result.Failure<string>();

        var dialog = await ShowDialogAndWaitAsync(new DialogWinModel(
            title ?? Resources.DIALOG__DEFAULT_ASK_TITLE,
            message,
            null,
            new[] { DialogButton.Ok, DialogButton.Cancel },
            null,
            new TextInputBox(true, "", "", '*')));

        var value = ((DialogWinModel)dialog.DataContext!).TextInputBox.Value;

        return Result.From(dialog.ViewModel!.Result is DialogButton.Ok, value ?? string.Empty);
    }

    private void CallOrDeffer(Action shower)
    {
        if (HostWindow is not null)
        {
            shower();
            return;
        }

        var added = false;

        lock (_deferred)
        {
            if (HostWindow is null)
            {
                _deferred.Add(shower);
                added = true;
            }
        }

        if (!added) shower();
    }

    private void ShowNotification(INotification context)
    {
        Dispatcher.UIThread.Invoke(() => NotificationManager!.Show(context));
    }

    private void ShowDialog(DialogWinModel context)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var dialog = new DialogWindow { DataContext = context };

            dialog.CorrectMainWindowFocusWhileOpened(_hostWindowProviderResolver()!);

            dialog.Closing += (_, _) => TryRemoveOpened(dialog);
            AddOpened(dialog);

            try
            {
                dialog.Show(HostWindow!);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Dialog window opening failed");
                TryRemoveOpened(dialog);
            }
        });
    }

    private async Task<DialogWindow> ShowDialogAndWaitAsync(DialogWinModel context)
    {
        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var dialog = new DialogWindow { DataContext = context };

            try
            {
                await dialog.ShowDialog(HostWindow!);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Dialog window showing failed");
            }

            return dialog;
        });
    }

    private void AddOpened(Window dialog)
    {
        lock (_opened)
        {
            _opened.Add(dialog);

            if (HostWindow is not null)
            {
                HostWindow.IsEnabled = false;
            }
        }
    }

    private void TryRemoveOpened(Window dialog)
    {
        lock (_opened)
        {
            _opened.Remove(dialog);

            if (HostWindow is not null)
            {
                HostWindow.IsEnabled = _opened.Count == 0;
            }
        }
    }
}