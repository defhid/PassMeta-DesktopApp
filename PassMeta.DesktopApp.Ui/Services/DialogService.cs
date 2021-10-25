using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Ui.Models.DialogWindow.Components;
using PassMeta.DesktopApp.Ui.ViewModels.Main;
using PassMeta.DesktopApp.Ui.Views.Main;

namespace PassMeta.DesktopApp.Ui.Services
{
    public class DialogService : IDialogService
    {
        private static Window? _win;
        private static List<DialogWindowViewModel> _deferred = new();

        public static async Task SetCurrentWindowAsync(Window window)
        {
            _win = window;

            var deferred = _deferred;
            _deferred = new List<DialogWindowViewModel>();
            
            foreach (var context in deferred)
            {
                try
                {
                    await new DialogWindow { DataContext = context }.ShowDialog(_win);
                }
                catch
                {
                    // ignored
                }
            }
        }
        
        public Task ShowInfoAsync(string message, string? title = null, string? more = null) 
            => _ShowOrDeferAsync(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_INFO_TITLE,
                message,
                more,
                new[] { DialogButton.Close },
                DialogWindowIcon.Info,
                null)
            );

        public Task ShowErrorAsync(string message, string? title = null, string? more = null)
            => _ShowOrDeferAsync(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_ERROR_TITLE,
                message,
                more,
                new[] { DialogButton.Close },
                DialogWindowIcon.Error,
                null)
            );

        public Task ShowFailureAsync(string message, string? more = null)
            => _ShowOrDeferAsync(new DialogWindowViewModel(
                Resources.DIALOG__DEFAULT_FAILURE_TITLE,
                message,
                more,
                new[] { DialogButton.Close },
                DialogWindowIcon.Failure,
                null)
            );

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

        public async Task<Result<string?>> AskStringAsync(string message, string? title = null)
        {
            if (_win is null) return new Result<string?>(false);

            var dialog = await _ShowAsync(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_ASK_TITLE,
                message,
                null,
                new[] { DialogButton.Ok, DialogButton.Cancel },
                null,
                new DialogWindowTextBox(true, "", "", null)));

            var value = ((DialogWindowViewModel)dialog.DataContext!).WindowTextBox.Value;

            return dialog.ResultButton == DialogButton.Ok && !string.IsNullOrEmpty(value)
                ? new Result<string?>(value)
                : new Result<string?>(false);
        }
        
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

            return dialog.ResultButton == DialogButton.Ok && !string.IsNullOrEmpty(value)
                ? new Result<string?>(value)
                : new Result<string?>(false);
        }
        
        private static Task _ShowOrDeferAsync(DialogWindowViewModel context)
        {
            if (_win is not null) return _ShowAsync(context);
            
            _deferred.Add(context);
            return Task.CompletedTask;
        }

        private static async Task<DialogWindow> _ShowAsync(DialogWindowViewModel context)
        {
            var dialog = new DialogWindow { DataContext = context };
            await dialog.ShowDialog(_win);
            return dialog;
        }
    }
}