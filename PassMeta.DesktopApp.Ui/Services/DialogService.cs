using Avalonia.Controls;
using Avalonia.Controls.Notifications;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.DialogWin;
using PassMeta.DesktopApp.Ui.Views.Windows;

namespace PassMeta.DesktopApp.Ui.Services;

/// <inheritdoc />
public class DialogService : IDialogService
{
    private readonly Func<INotificationManager?> _notificationManagerResolver;
    private readonly ILogsWriter _logger;
    private readonly List<Action> _deferred = new();
    private readonly List<Window> _opened = new();

    public DialogService(Func<INotificationManager?> notificationManagerResolver, ILogsWriter logger)
    {
        _notificationManagerResolver = notificationManagerResolver;
        _logger = logger;
    }

    private INotificationManager? NotificationManager => _notificationManagerResolver();

    /// <inheritdoc />
    public void Flush()
    {
        if (App.App.MainWindow is null)
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
                ShowNotification(new Notification(title,
                    message + (more is null ? string.Empty : Environment.NewLine + $"[{more}]"),
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
                    TimeSpan.FromSeconds(8)));
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
        if (App.App.MainWindow is null) return Result.Failure();

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
        if (App.App.MainWindow is null) return Result.Failure<string>();

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
        if (App.App.MainWindow is null) return Result.Failure<string>();

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
        if (App.App.MainWindow is not null)
        {
            shower();
            return;
        }
            
        var added = false;
                
        lock (_deferred)
        {
            if (App.App.MainWindow is null)
            {
                _deferred.Add(shower);
                added = true;
            }
        }

        if (!added) shower();
    }
        
    private void ShowNotification(INotification context)
    {
        NotificationManager!.Show(context);
    }

    private void ShowDialog(DialogWinModel context)
    {
        var dialog = new DialogWindow { DataContext = context };
            
        dialog.CorrectMainWindowFocusWhileOpened();

        dialog.Closing += (_, _) => TryRemoveOpened(dialog);
        AddOpened(dialog);

        try
        {
            dialog.Show(App.App.MainWindow);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Dialog window opening failed");
            TryRemoveOpened(dialog);
        }
    }
        
    private async Task<DialogWindow> ShowDialogAndWaitAsync(DialogWinModel context)
    {
        var dialog = new DialogWindow { DataContext = context };

        try
        {
            await dialog.ShowDialog(App.App.MainWindow);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Dialog window showing failed");
        }

        return dialog;
    }

    private void AddOpened(Window dialog)
    {
        lock (_opened)
        {
            _opened.Add(dialog);
            App.App.MainWindow!.IsEnabled = false;
        }
    }
        
    private void TryRemoveOpened(Window dialog)
    {
        lock (_opened)
        {
            _opened.Remove(dialog);
            App.App.MainWindow!.IsEnabled = _opened.Count == 0;
        }
    }
}