using PassMeta.DesktopApp.Core;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class AuthRequiredViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/auth-required";

        public string Title => Resources.AUTH_REQUIRED__TITLE;

        public AuthRequiredViewModel(IScreen hostScreen) : base(hostScreen)
        {
        }
    }
}