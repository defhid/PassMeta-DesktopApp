using System.Threading.Tasks;
using System.Windows.Input;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models.Entities.Request;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.ViewModels.Base;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class AuthViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/auth";

        public string? Login { get; set; }
        
        public string? Password { get; set; }

        public ICommand SignInCommand { get; }
        
        public ICommand SignUpCommand { get; }

        public AuthViewModel(IScreen hostScreen) : base(hostScreen)
        {
            SignInCommand = ReactiveCommand.CreateFromTask(_SignInAsync);
            SignUpCommand = ReactiveCommand.CreateFromTask(_SignUpAsync);
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

        public override Task RefreshAsync()
        {
            Navigate();
            return Task.CompletedTask;
        }
        
        private async Task _SignInAsync()
        {
            var service = Locator.Current.GetService<IAuthService>()!;
            var result = await service.SignInAsync(new SignInPostData(Login ?? "", Password ?? ""));
            if (result.Ok)
                NavigateTo<AccountViewModel>();
        }
        
        private async Task _SignUpAsync()
        {
            var service = Locator.Current.GetService<IAuthService>()!;
            var result = await service.SignUpAsync(new SignUpPostData(
                Login ?? "", Password ?? "", "Unknown", "Unknown"));
            if (result.Ok)
                NavigateTo<AccountViewModel>();
        }
    }
}