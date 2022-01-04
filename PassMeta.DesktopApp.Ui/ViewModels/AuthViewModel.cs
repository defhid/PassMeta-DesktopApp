namespace PassMeta.DesktopApp.Ui.ViewModels
{
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models.Dto.Request;
    using DesktopApp.Core.Utils;
    using DesktopApp.Ui.ViewModels.Base;
    
    using System.Threading.Tasks;
    using System.Windows.Input;
    using ReactiveUI;
    using Splat;
    
    public class AuthViewModel : ViewModelPage
    {
        private readonly IAuthService _authService = Locator.Current.GetService<IAuthService>()!;
        
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
            if (AppContext.Current.User is not null)
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
            var data = new SignInPostData(Login ?? "", Password ?? "");
            var result = await _authService.SignInAsync(data);
            if (result.Ok)
                NavigateTo<AccountViewModel>();
        }
        
        private async Task _SignUpAsync()
        {
            var data = new SignUpPostData(Login ?? "", Password ?? "", "Unknown", "Unknown");
            var result = await _authService.SignUpAsync(data);
            if (result.Ok)
                NavigateTo<AccountViewModel>();
        }
    }
}