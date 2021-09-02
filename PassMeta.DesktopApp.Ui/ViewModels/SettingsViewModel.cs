using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class SettingsViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/settings";

        public SettingsViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;
        }
    }
}