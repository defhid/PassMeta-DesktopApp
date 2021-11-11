namespace PassMeta.DesktopApp.Ui.Services
{
    using DesktopApp.Common;
    using DesktopApp.Common.Enums;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models;
    using DesktopApp.Ui.Models.DialogWindow.Components;
    using DesktopApp.Ui.ViewModels.Main;
    using DesktopApp.Ui.Views.Main;
    
    using Splat;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    
    using Avalonia.Controls;
    using Avalonia.Controls.Notifications;

    /// <inheritdoc />
    public class DialogService : IDialogService
    {
        private static Window? _win;
        private static List<Func<Task>> _deferred = new();
        private readonly ILogService _logger;

        private static INotificationManager? NotificationManager => Locator.Current.GetService<INotificationManager>();

        public DialogService(ILogService logger)
        {
            _logger = logger;
        }

        public static async Task SetCurrentWindowAsync(Window window)
        {
            _win = window;

            var deferred = _deferred;
            _deferred = new List<Func<Task>>();
            
            foreach (var shower in deferred)
            {
                try { await shower(); }
                catch
                {
                    // ignored
                }
            }
        }

        /// <inheritdoc />
        public Task ShowInfoAsync(string message, string? title = null, string? more = null, 
            DialogPresenter defaultPresenter = DialogPresenter.PopUp)
        {
            _logger.Info(message + (more is null ? string.Empty : Environment.NewLine + $"[{more}]"));
            return _CallOrDefferAsync(() =>
            {
                if (defaultPresenter is DialogPresenter.PopUp && NotificationManager is not null)
                {
                    _Show(new Notification(title,
                        message + (more is null ? string.Empty : Environment.NewLine + $"[{more}]"),
                        NotificationType.Information, TimeSpan.FromSeconds(2)));

                    return Task.CompletedTask;
                }
                
                return _ShowAsync(new DialogWindowViewModel(
                    title ?? Resources.DIALOG__DEFAULT_INFO_TITLE,
                    message,
                    more,
                    new[] { DialogButton.Close },
                    DialogWindowIcon.Info,
                    null));
            });
        }

        /// <inheritdoc />
        public Task ShowErrorAsync(string message, string? title = null, string? more = null, 
            DialogPresenter defaultPresenter = DialogPresenter.PopUp)
        {
            _logger.Error(message + (more is null ? string.Empty : Environment.NewLine + $"[{more}]"));
            return _CallOrDefferAsync(() =>
            {
                if (defaultPresenter is DialogPresenter.PopUp && NotificationManager is not null)
                {
                    _Show(new Notification(
                        title ?? Resources.DIALOG__DEFAULT_ERROR_TITLE,
                        message + (more is null ? string.Empty : Environment.NewLine + $"[{more}]"),
                        NotificationType.Error,
                        TimeSpan.FromSeconds(8)));

                    return Task.CompletedTask;
                }

                return _ShowAsync(new DialogWindowViewModel(
                    title ?? Resources.DIALOG__DEFAULT_ERROR_TITLE,
                    message,
                    more,
                    new[] { DialogButton.Close },
                    DialogWindowIcon.Error,
                    null));
            });
        }

        /// <inheritdoc />
        public Task ShowFailureAsync(string message, string? more = null, 
            DialogPresenter defaultPresenter = DialogPresenter.Window)
        {
            _logger.Warning(message + (more is null ? string.Empty : Environment.NewLine + $"[{more}]"));
            
            return _CallOrDefferAsync(() =>
            {
                if (defaultPresenter is DialogPresenter.PopUp && NotificationManager is not null)
                {
                    _Show(new Notification(
                        Resources.DIALOG__DEFAULT_FAILURE_TITLE,
                        message + (more is null ? string.Empty : Environment.NewLine + $"[{more}]"),
                        NotificationType.Error,
                        TimeSpan.FromSeconds(8)));

                    return Task.CompletedTask;
                }
                
                return _ShowAsync(new DialogWindowViewModel(
                    Resources.DIALOG__DEFAULT_FAILURE_TITLE,
                    message,
                    more,
                    new[] { DialogButton.Close },
                    DialogWindowIcon.Failure,
                    null)
                );
            });
        }

        /// <inheritdoc />
        public async Task<Result> ConfirmAsync(string message, string? title = null)
        {
            if (_win is null) return Result.Failure;

            var dialog = await _ShowAsync(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_CONFIRM_TITLE,
                message,
                null,
                new[] { DialogButton.Yes, DialogButton.Cancel },
                DialogWindowIcon.Confirm,
                null));

            return new Result(dialog.ResultButton == DialogButton.Yes);
        }

        /// <inheritdoc />
        public async Task<Result<string?>> AskStringAsync(string message, string? title = null, string? defaultValue = null)
        {
            if (_win is null) return new Result<string?>(false);

            var dialog = await _ShowAsync(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_ASK_TITLE,
                message,
                null,
                new[] { DialogButton.Ok, DialogButton.Cancel },
                null,
                new DialogWindowTextBox(true, "", defaultValue, null)));

            var value = ((DialogWindowViewModel)dialog.DataContext!).WindowTextBox.Value?.Trim();

            return dialog.ResultButton == DialogButton.Ok
                ? new Result<string?>(value ?? string.Empty)
                : new Result<string?>(false);
        }
        
        /// <inheritdoc />
        public async Task<Result<string?>> AskPasswordAsync(string message, string? title = null)
        {
            if (_win is null) return new Result<string?>(false);

            var dialog = await _ShowAsync(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_ASK_TITLE,
                message,
                null,
                new[] { DialogButton.Ok, DialogButton.Cancel },
                null,
                new DialogWindowTextBox(true, "", "", '*')));

            var value = ((DialogWindowViewModel)dialog.DataContext!).WindowTextBox.Value;

            return dialog.ResultButton == DialogButton.Ok
                ? new Result<string?>(value ?? string.Empty)
                : new Result<string?>(false);
        }

        private static Task _CallOrDefferAsync(Func<Task> shower)
        {
            if (_win is not null) return shower();
            
            _deferred.Add(shower);
            return Task.CompletedTask;
        }
        
        private static void _Show(INotification context)
        {
            NotificationManager!.Show(context);
        }

        private static async Task<DialogWindow> _ShowAsync(DialogWindowViewModel context)
        {
            var dialog = new DialogWindow { DataContext = context };
            await dialog.ShowDialog(_win);
            return dialog;
        }
    }
}