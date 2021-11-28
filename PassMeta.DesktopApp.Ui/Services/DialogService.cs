namespace PassMeta.DesktopApp.Ui.Services
{
    using DesktopApp.Common;
    using DesktopApp.Common.Enums;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models;
    using DesktopApp.Ui.ViewModels.Main;
    using DesktopApp.Ui.Views.Main;
    
    using Splat;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    
    using Avalonia.Controls;
    using Avalonia.Controls.Notifications;
    using Models.Components.DialogWindow;
    using Models.Enums;

    /// <inheritdoc />
    public class DialogService : IDialogService
    {
        private static INotificationManager? NotificationManager => Locator.Current.GetService<INotificationManager>();
        private static List<Func<Task>> _deferred = new();
        
        private readonly ILogService _logger;
        
        static DialogService()
        {
            MainWindow.CurrentChanged += _SetCurrentWindowAsync;
        }

        public DialogService(ILogService logger)
        {
            _logger = logger;
        }

        private static async Task _SetCurrentWindowAsync(Window window)
        {
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
            _logger.Error(message + (string.IsNullOrWhiteSpace(more) ? string.Empty : Environment.NewLine + $"[{more}]"));
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
            _logger.Warning(message + (string.IsNullOrWhiteSpace(more) ? string.Empty : Environment.NewLine + $"[{more}]"));
            
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
            if (MainWindow.Current is null) return Result.Failure();

            var dialog = await _ShowAsync(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_CONFIRM_TITLE,
                message,
                null,
                new[] { DialogButton.Yes, DialogButton.Cancel },
                DialogWindowIcon.Confirm,
                null));

            return Result.From(dialog.ResultButton == DialogButton.Yes);
        }

        /// <inheritdoc />
        public async Task<Result<string>> AskStringAsync(string message, string? title = null, string? defaultValue = null)
        {
            if (MainWindow.Current is null) return Result.Failure<string>();

            var dialog = await _ShowAsync(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_ASK_TITLE,
                message,
                null,
                new[] { DialogButton.Ok, DialogButton.Cancel },
                null,
                new DialogWindowTextBox(true, "", defaultValue, null)));

            var value = ((DialogWindowViewModel)dialog.DataContext!).WindowTextBox.Value?.Trim();

            return Result.From(dialog.ResultButton == DialogButton.Ok, value ?? string.Empty);
        }
        
        /// <inheritdoc />
        public async Task<Result<string>> AskPasswordAsync(string message, string? title = null)
        {
            if (MainWindow.Current is null) return Result.Failure<string>();

            var dialog = await _ShowAsync(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_ASK_TITLE,
                message,
                null,
                new[] { DialogButton.Ok, DialogButton.Cancel },
                null,
                new DialogWindowTextBox(true, "", "", '*')));

            var value = ((DialogWindowViewModel)dialog.DataContext!).WindowTextBox.Value;
            
            return Result.From(dialog.ResultButton == DialogButton.Ok, value ?? string.Empty);
        }

        private static Task _CallOrDefferAsync(Func<Task> shower)
        {
            if (MainWindow.Current is not null) return shower();
            
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
            await dialog.ShowDialog(MainWindow.Current);
            return dialog;
        }
    }
}