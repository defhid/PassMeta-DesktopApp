using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models;

namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    public interface IDialogService
    {
        public Task ShowInfoAsync(string message, string? title = null, string? more = null);

        public Task ShowErrorAsync(string message, string? title = null, string? more = null);
        
        public Task ShowFailureAsync(string message, string? more = null);

        public Task<Result> ConfirmAsync(string message, string? title = null);

        public Task<Result<string?>> AskStringAsync(string message, string? title = null);
        
        public Task<Result<string?>> AskPasswordAsync(string message, string? title = null);
    }
}