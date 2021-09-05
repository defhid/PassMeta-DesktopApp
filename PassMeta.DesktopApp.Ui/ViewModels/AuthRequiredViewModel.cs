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
    }
}