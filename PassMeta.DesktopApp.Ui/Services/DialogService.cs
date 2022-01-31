namespace PassMeta.DesktopApp.Ui.Services
{
    using DesktopApp.Common;
    using DesktopApp.Common.Enums;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models;
    using DesktopApp.Ui.Views.Main;
    
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    
    using Avalonia.Controls;
    using Avalonia.Controls.Notifications;
    using Constants;
    using Core;
    using Utils.Extensions;
    using ViewModels.Main.DialogWindow;
    using ViewModels.Main.DialogWindow.Components;

    /// <inheritdoc />
    public class DialogService : IDialogService
    {
        private static INotificationManager? NotificationManager => EnvironmentContainer.Resolve<INotificationManager>();
        private static ILogService Logger => EnvironmentContainer.Resolve<ILogService>();
        
        private static readonly List<Action> Deferred = new();
        private static readonly List<Window> Opened = new();
        
        static DialogService()
        {
            MainWindow.CurrentChanged += _SetCurrentWindowAsync;
        }

        private static Task _SetCurrentWindowAsync(Window window)
        {
            lock (Deferred)
            {
                foreach (var shower in Deferred)
                {
                    try
                    {
                        shower();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Deferred dialog showing failed");
                    }
                }
                Deferred.Clear();
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void ShowInfo(string message, string? title = null, string? more = null, 
            DialogPresenter defaultPresenter = DialogPresenter.PopUp)
        {
            _CallOrDeffer(() =>
            {
                if (defaultPresenter is DialogPresenter.PopUp && NotificationManager is not null)
                {
                    _ShowNotification(new Notification(title,
                        message + (more is null ? string.Empty : Environment.NewLine + $"[{more}]"),
                        NotificationType.Information, TimeSpan.FromSeconds(2)));
                }
                else
                {
                    _ShowDialog(new DialogWindowViewModel(
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
            Logger.Error(message + (string.IsNullOrWhiteSpace(more) ? string.Empty : Environment.NewLine + $"[{more}]"));
            _CallOrDeffer(() =>
            {
                if (defaultPresenter is DialogPresenter.PopUp && NotificationManager is not null)
                {
                    _ShowNotification(new Notification(
                        title ?? Resources.DIALOG__DEFAULT_ERROR_TITLE,
                        message + (more is null ? string.Empty : Environment.NewLine + $"[{more}]"),
                        NotificationType.Error,
                        TimeSpan.FromSeconds(8)));
                }
                else
                {
                    _ShowDialog(new DialogWindowViewModel(
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
        public void ShowFailure(string message, string? more = null, 
            DialogPresenter defaultPresenter = DialogPresenter.Window)
        {
            Logger.Warning(message + (string.IsNullOrWhiteSpace(more) ? string.Empty : Environment.NewLine + $"[{more}]"));
            _CallOrDeffer(() =>
            {
                if (defaultPresenter is DialogPresenter.PopUp && NotificationManager is not null)
                {
                    _ShowNotification(new Notification(
                        Resources.DIALOG__DEFAULT_FAILURE_TITLE,
                        message + (more is null ? string.Empty : Environment.NewLine + $"[{more}]"),
                        NotificationType.Error,
                        TimeSpan.FromSeconds(8)));
                }
                else
                {
                    _ShowDialog(new DialogWindowViewModel(
                        Resources.DIALOG__DEFAULT_FAILURE_TITLE,
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
        public async Task<Result> ConfirmAsync(string message, string? title = null)
        {
            if (MainWindow.Current is null) return Result.Failure();

            var dialog = await _ShowDialogAsync(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_CONFIRM_TITLE,
                message,
                null,
                new[] { DialogButton.Yes, DialogButton.Cancel },
                DialogWindowIcon.Confirm,
                null));

            return Result.From(dialog.ResultButton == DialogButton.Yes);
        }

        /// <inheritdoc />
        /// <remarks>Use only when the main window has been rendered.</remarks>
        public async Task<Result<string>> AskStringAsync(string message, string? title = null, string? defaultValue = null)
        {
            if (MainWindow.Current is null) return Result.Failure<string>();

            var dialog = await _ShowDialogAsync(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_ASK_TITLE,
                message,
                null,
                new[] { DialogButton.Ok, DialogButton.Cancel },
                null,
                new TextInputBox(true, "", defaultValue, null)));

            var value = ((DialogWindowViewModel)dialog.DataContext!).WindowTextInputBox.Value?.Trim();

            return Result.From(dialog.ResultButton == DialogButton.Ok, value ?? string.Empty);
        }
        
        /// <inheritdoc />
        /// <remarks>Use only when the main window has been rendered.</remarks>
        public async Task<Result<string>> AskPasswordAsync(string message, string? title = null)
        {
            if (MainWindow.Current is null) return Result.Failure<string>();

            var dialog = await _ShowDialogAsync(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_ASK_TITLE,
                message,
                null,
                new[] { DialogButton.Ok, DialogButton.Cancel },
                null,
                new TextInputBox(true, "", "", '*')));

            var value = ((DialogWindowViewModel)dialog.DataContext!).WindowTextInputBox.Value;
            
            return Result.From(dialog.ResultButton == DialogButton.Ok, value ?? string.Empty);
        }

        private static void _CallOrDeffer(Action shower)
        {
            if (MainWindow.Current is not null)
            {
                shower();
                return;
            }
            
            var added = false;
                
            lock (Deferred)
            {
                if (MainWindow.Current is null)
                {
                    Deferred.Add(shower);
                    added = true;
                }
            }

            if (!added) shower();
        }
        
        private static void _ShowNotification(INotification context)
        {
            NotificationManager!.Show(context);
        }

        private static void _ShowDialog(DialogWindowViewModel context)
        {
            var dialog = new DialogWindow { DataContext = context };
            
            dialog.CorrectMainWindowFocusWhileOpened();

            dialog.Closing += (_, _) => _TryRemoveOpened(dialog);
            _AddOpened(dialog);

            try
            {
                dialog.Show(MainWindow.Current);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Dialog window opening failed");
                _TryRemoveOpened(dialog);
            }
        }
        
        private static async Task<DialogWindow> _ShowDialogAsync(DialogWindowViewModel context)
        {
            var dialog = new DialogWindow { DataContext = context };

            try
            {
                await dialog.ShowDialog(MainWindow.Current);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Dialog window showing failed");
            }
            
            return dialog;
        }

        private static void _AddOpened(Window dialog)
        {
            lock (Opened)
            {
                Opened.Add(dialog);
                MainWindow.Current!.IsEnabled = false;
            }
        }
        
        private static void _TryRemoveOpened(Window dialog)
        {
            lock (Opened)
            {
                Opened.Remove(dialog);
                MainWindow.Current!.IsEnabled = Opened.Count == 0;
            }
        }
    }
}