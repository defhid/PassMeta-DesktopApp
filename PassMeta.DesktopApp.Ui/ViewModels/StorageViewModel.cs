using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class StorageViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/storage";

        public StorageViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;
        }
    }
}