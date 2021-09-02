using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class GeneratorViewModel : ViewModelBase, IRoutableViewModel
    {
        public string UrlPathSegment => "/generator";
        public IScreen HostScreen { get; }

        public GeneratorViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;
        }
    }
}