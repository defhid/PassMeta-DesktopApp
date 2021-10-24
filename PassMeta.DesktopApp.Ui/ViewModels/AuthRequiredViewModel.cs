using System.Threading.Tasks;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.ViewModels.Base;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class AuthRequiredViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/auth-required";

        public AuthRequiredViewModel(IScreen hostScreen) : base(hostScreen)
        {
        }

        public override Task RefreshAsync()
        {
            if (AppConfig.Current.User is not null)
            {
                HostScreen.Router.NavigateBack.Execute();
            }
            
            return Task.CompletedTask;
        }
    }
}