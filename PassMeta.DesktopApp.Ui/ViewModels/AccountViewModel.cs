using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class AccountViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/account";

        public AccountViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;
        }
    }
}