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
        private static int _count;
        private static readonly List<DialogWindowViewModel> Deferred = new();
        
        public static int ActiveCount
        {
            get => _count;
            private set
            {
                _count = value;
                if (_win is not null)
                    _win.IsEnabled = value == 0;
            }
        }

        public static async Task SetCurrentWindowAsync(Window window)
        {
            _win = window;
            
            var deferred = Deferred.ToArray();
                
            Deferred.Clear();
            ActiveCount += deferred.Length;
            
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

            ActiveCount -= deferred.Length;
        }
        
        public void ShowInfo(string message, string? title = null, string? more = null)
        {
            _ShowOrDefer(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_INFO_TITLE,
                message,
                more,
                new[] { DialogButton.Close },
                DialogWindowIcon.Info,
                null)
            );
        }

        public void ShowError(string message, string? title = null, string? more = null)
        {
            _ShowOrDefer(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_ERROR_TITLE,
                message,
                more,
                new[] { DialogButton.Close },
                DialogWindowIcon.Error,
                null)
            );
        }

        public void ShowFailure(string message, string? more = null)
        {
            _ShowOrDefer(new DialogWindowViewModel(
                Resources.DIALOG__DEFAULT_FAILURE_TITLE,
                message,
                more,
                new[] { DialogButton.Close },
                DialogWindowIcon.Failure,
                null)
            );
        }

        public async Task<Result> Confirm(string message, string? title = null)
        {
            if (_win is null) return Result.Failure;
            
            var dialog = new DialogWindow 
            { 
                DataContext = new DialogWindowViewModel(
                    title ?? Resources.DIALOG__DEFAULT_CONFIRM_TITLE,
                    message,
                    null,
                    new[] { DialogButton.Yes, DialogButton.Cancel },
                    DialogWindowIcon.Confirm,
                    null)
            };
            dialog.Closed += (_, _) =>
            {
                ActiveCount -= 1;
            };
            
            ActiveCount += 1;
            await dialog.ShowDialog(_win);
            
            return new Result(dialog.ResultButton == DialogButton.Yes);
        }

        public async Task<Result<string?>> AskString(string message, string? title = null)
        {
            if (_win is null) return new Result<string?>(false);

            var dialog = new DialogWindow
            {
                DataContext = new DialogWindowViewModel(
                    title ?? Resources.DIALOG__DEFAULT_ASK_TITLE,
                    message,
                    null,
                    new[] { DialogButton.Ok, DialogButton.Cancel },
                    null,
                    new DialogWindowTextBox(true, "", "", '*'))
            };
            
            dialog.Closed += (_, _) =>
            {
                ActiveCount -= 1;
            };
            
            ActiveCount += 1;
            await dialog.ShowDialog(_win);

            var value = ((DialogWindowViewModel)dialog.DataContext).WindowTextBox.Value;

            return dialog.ResultButton == DialogButton.Ok && !string.IsNullOrEmpty(value)
                ? new Result<string?>(value)
                : new Result<string?>(false);
        }
        
        private static void _ShowOrDefer(DialogWindowViewModel context)
        {
            if (_win is null)
            {
                Deferred.Add(context);
            }
            else
            {
                _Show(context);
            }
        }

        private static DialogWindow _Show(DialogWindowViewModel context)
        {
            var dialog = new DialogWindow { DataContext = context };
            dialog.Closed += (_, _) =>
            {
                ActiveCount -= 1;
            };
            
            dialog.Show();
            ActiveCount += 1;

            return dialog;
        }
    }
}