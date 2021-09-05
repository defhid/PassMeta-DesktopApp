using PassMeta.DesktopApp.Core;
using PassMeta.DesktopApp.Core.Utils;
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
        
        public string? Login { get; set; }
        
        public string? Password { get; set; }

        public AuthViewModel(IScreen hostScreen) : base(hostScreen)
        {
        }
        
        public override void Navigate()
        {
            if (AppConfig.Current.User is not null)
            {
                NavigateTo<AccountViewModel>();
            }
            else
            {
                base.Navigate();
            }
        }
    }
}