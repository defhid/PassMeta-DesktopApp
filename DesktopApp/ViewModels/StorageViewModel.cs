using ReactiveUI;

namespace DesktopApp.ViewModels
{
    public class StorageViewModel : ViewModelBase, IRoutableViewModel
    {
        public string UrlPathSegment => "/storage";
        public IScreen HostScreen { get; }

        public StorageViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;
        }
    }
}