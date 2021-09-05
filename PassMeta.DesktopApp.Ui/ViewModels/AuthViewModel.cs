using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.ViewModels.Base;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class AuthViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/auth";

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