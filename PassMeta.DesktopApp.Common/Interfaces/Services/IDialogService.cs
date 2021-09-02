using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models;

namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    public interface IDialogService
    {
        public Task ShowInfoAsync(string message, string title = null, string more = null);

        public Task ShowErrorAsync(string message, string title = null, string more = null);
        
        public Task ShowFailureAsync(string message);

        public Task<Result<bool>> Ask(string message, string title = null);

        public Task<Result<string>> AskString(string message, string title = null);
    }
}