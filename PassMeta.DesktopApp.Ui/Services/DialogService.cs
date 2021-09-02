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
        
        public async Task ShowInfoAsync(string message, string? title = null, string? more = null)
        {
            await _ShowOrDeferAsync(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_INFO_TITLE,
                message,
                new[] { DialogButton.Close },
                DialogWindowIcon.Info,
                null)
            );
        }

        public async Task ShowErrorAsync(string message, string? title = null, string? more = null)
        {
            await _ShowOrDeferAsync(new DialogWindowViewModel(
                title ?? Resources.DIALOG__DEFAULT_ERROR_TITLE,
                message,
                new[] { DialogButton.Close },
                DialogWindowIcon.Error,
                null)
            );
        }

        public async Task ShowFailureAsync(string message)
        {
            await _ShowOrDeferAsync(new DialogWindowViewModel(
                Resources.DIALOG__DEFAULT_FAILURE_TITLE,
                message,
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
        
        private static async Task _ShowOrDeferAsync(DialogWindowViewModel context)
        {
            if (_win is null)
            {
                Deferred.Add(context);
            }
            else
            {
                await _ShowAsync(context);
            }
        }

        private static async Task<DialogWindow> _ShowAsync(DialogWindowViewModel context)
        {
            var dialog = new DialogWindow { DataContext = context };
            
            ActiveCount += 1;
            try
            {
                await dialog.ShowDialog(_win);
            }
            finally
            {
                ActiveCount -= 1;
            }

            return dialog;
        }
    }
}