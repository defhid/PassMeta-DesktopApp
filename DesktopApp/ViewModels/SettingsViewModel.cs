using ReactiveUI;

namespace DesktopApp.ViewModels
{
    public class SettingsViewModel : ViewModelBase, IRoutableViewModel
    {
        public string UrlPathSegment => "/settings";
        public IScreen HostScreen { get; }

        public SettingsViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;
        }
    }
}