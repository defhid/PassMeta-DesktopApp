using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public abstract class ViewModelPage : ReactiveObject, IRoutableViewModel
    {
        public abstract string? UrlPathSegment { get; }
        public IScreen HostScreen { get; protected set; }
    }
}