using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models;

namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    public interface IDialogService
    {
        public void ShowInfo(string message, string title = null, string more = null);

        public void ShowError(string message, string title = null, string more = null);
        
        public void ShowFailure(string message);

        public Task<Result<bool>> Ask(string message, string title = null);

        public Task<Result<string>> AskString(string message, string title = null);
    }
}