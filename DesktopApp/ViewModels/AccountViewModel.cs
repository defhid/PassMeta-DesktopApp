using ReactiveUI;

namespace DesktopApp.ViewModels
{
    public class AccountViewModel : ViewModelBase, IRoutableViewModel
    {
        public string UrlPathSegment => "/account";
        public IScreen HostScreen { get; }

        public AccountViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;
        }
    }
}