using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models;

namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    using Enums;

    public interface IDialogService
    {
        public Task ShowInfoAsync(string message, string? title = null, string? more = null, DialogPresenter defaultPresenter = DialogPresenter.PopUp);

        public Task ShowErrorAsync(string message, string? title = null, string? more = null, DialogPresenter defaultPresenter = DialogPresenter.PopUp);
        
        public Task ShowFailureAsync(string message, string? more = null, DialogPresenter defaultPresenter = DialogPresenter.Window);

        public Task<Result> ConfirmAsync(string message, string? title = null);

        public Task<Result<string?>> AskStringAsync(string message, string? title = null, string? defaultValue = null);
        
        public Task<Result<string?>> AskPasswordAsync(string message, string? title = null);
    }
}