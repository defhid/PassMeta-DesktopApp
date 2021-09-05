using PassMeta.DesktopApp.Core.Utils;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class AccountViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/account";

        public AccountViewModel(IScreen hostScreen) : base(hostScreen)
        {
        }

        public override void Navigate()
        {
            if (AppConfig.Current.User is null)
            {
                NavigateTo<AuthViewModel>();
            }
            else
            {
                base.Navigate();
            }
        }
    }
}