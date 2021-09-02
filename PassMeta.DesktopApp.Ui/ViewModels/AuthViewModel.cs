using PassMeta.DesktopApp.Core;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class AuthViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/auth";
        public string Title => Resources.AUTH__TITLE;

        public string LoginInputPlaceholder => Resources.AUTH__LOGIN_PLACEHOLDER;

        public string PasswordInputPlaceholder => Resources.AUTH__PASSWORD_PLACEHOLDER;

        public string SignInBtnContent => Resources.AUTH__SIGN_IN_BTN;
        
        public string SignUpBtnContent => Resources.AUTH__SIGN_UP_BTN;

        public AuthViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;
        }
    }
}