namespace PassMeta.DesktopApp.Ui.ViewModels
{
    using DesktopApp.Common.Models.Dto.Request;
    using DesktopApp.Core.Utils;
    using DesktopApp.Ui.ViewModels.Base;
    using DesktopApp.Ui.Views.Main;
    
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Common.Abstractions.Services;
    using Core;
    using ReactiveUI;

    public class AuthViewModel : PageViewModel
    {
        private static IAuthService AuthService => EnvironmentContainer.Resolve<IAuthService>();

        public string? Login { get; set; }
        
        public string? Password { get; set; }

        public ICommand SignInCommand { get; }
        
        public ICommand SignUpCommand { get; }

        public AuthViewModel(IScreen hostScreen) : base(hostScreen)
        {
            SignInCommand = ReactiveCommand.CreateFromTask(_SignInAsync);
            SignUpCommand = ReactiveCommand.CreateFromTask(_SignUpAsync);
        }
        
        public override void TryNavigate()
        {
            if (AppContext.Current.User is not null)
            {
                TryNavigateTo<AccountViewModel>();
            }
            else
            {
                base.TryNavigate();
            }
        }

        public override Task RefreshAsync()
        {
            TryNavigate();
            return Task.CompletedTask;
        }
        
        private async Task _SignInAsync()
        {
            using var preloader = MainWindow.Current!.StartPreloader();
            
            var data = new SignInPostData(Login?.Trim() ?? "", Password ?? "");
            var result = await AuthService.SignInAsync(data);
            if (result.Ok)
                TryNavigateTo<AccountViewModel>();
        }
        
        private async Task _SignUpAsync()
        {
            using var preloader = MainWindow.Current!.StartPreloader();
            
            var data = new SignUpPostData(Login?.Trim() ?? "", Password ?? "", "Unknown");
            var result = await AuthService.SignUpAsync(data);
            if (result.Ok)
                TryNavigateTo<AccountViewModel>();
        }
    }
}