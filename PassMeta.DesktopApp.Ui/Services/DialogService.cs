using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Core;
using PassMeta.DesktopApp.Ui.Models.DialogWindow.Components;
using PassMeta.DesktopApp.Ui.ViewModels;
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

        public void ShowFailure(string message)
        {
            _ShowOrDefer(new DialogWindowViewModel(
                Resources.DIALOG__DEFAULT_FAILURE_TITLE,
                message,
                null,
                new[] { DialogButton.Close },
                DialogWindowIcon.Failure,
                null)
            );
        }

        public Task<Result<bool>> Ask(string message, string? title = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<Result<string>> AskString(string message, string? title = null)
        {
            throw new System.NotImplementedException();
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