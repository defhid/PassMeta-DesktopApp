namespace PassMeta.DesktopApp.Ui.ViewModels
{
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models.Dto.Request;
    using DesktopApp.Ui.ViewModels.Base;
    using DesktopApp.Ui.Views.Main;
    
    using AppContext = Core.Utils.AppContext;
    
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;
    
    using Avalonia.Controls;
    using Avalonia.Media;
    using ReactiveUI;
    using Splat;

    public class AccountViewModel : ViewModelPage
    {
        private static IAccountService AccountService => Locator.Current.GetService<IAccountService>()!;
        private static IAuthService AuthService => Locator.Current.GetService<IAuthService>()!;
        
        public override string UrlPathSegment => "/account";

        public override ContentControl[] RightBarButtons => new ContentControl[]
        {
            new Button
            {
                Opacity = 0.8,
                Content = "\uF3B1",
                Command = SignOutCommand
            }
        };

        private string? _firstName;
        public string? FirstName
        {
            get => _firstName;
            set => this.RaiseAndSetIfChanged(ref _firstName, value);
        }

        private string? _lastName;
        public string? LastName
        {
            get => _lastName;
            set => this.RaiseAndSetIfChanged(ref _lastName, value);
        }
        
        private string? _login;
        public string? Login
        {
            get => _login;
            set => this.RaiseAndSetIfChanged(ref _login, value);
        }

        private string? _password;
        public string? Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        private string? _passwordConfirm;
        public string? PasswordConfirm
        {
            get => _passwordConfirm;
            set => this.RaiseAndSetIfChanged(ref _passwordConfirm, value);
        }
        
        private IBrush? _passwordConfirmLabelForeground;
        public IBrush? PasswordConfirmLabelForeground
        {
            get => _passwordConfirmLabelForeground;
            set => this.RaiseAndSetIfChanged(ref _passwordConfirmLabelForeground, value);
        }

        private bool _isPasswordConfirmVisible;
        public bool IsPasswordConfirmVisible
        {
            get => _isPasswordConfirmVisible;
            set => this.RaiseAndSetIfChanged(ref _isPasswordConfirmVisible, value);
        }

        private bool _isBtnSaveVisible;
        public bool IsBtnSaveVisible
        {
            get => _isBtnSaveVisible;
            private set => this.RaiseAndSetIfChanged(ref _isBtnSaveVisible, value);
        }

        public ICommand SignOutCommand => ReactiveCommand.CreateFromTask(_SignOutAsync);

        public ICommand SaveCommand => ReactiveCommand.CreateFromTask(_SaveAsync);
        
        public AccountViewModel(IScreen hostScreen) : base(hostScreen)
        {
            if (AppContext.Current.User is null) return;
            
            _Refresh();
            this.WhenAnyValue(
                    vm => vm.FirstName,
                    vm => vm.LastName,
                    vm => vm.Login,
                    vm => vm.Password,
                    vm => vm.PasswordConfirm)
                .Subscribe(data =>
                    {
                        var user = AppContext.Current.User;
                        var changed = data.Item1 != user.FirstName || 
                                          data.Item2 != user.LastName ||
                                          data.Item3 != user.Login ||
                                          !string.IsNullOrEmpty(data.Item4);
                        
                        IsPasswordConfirmVisible = changed;
                        PasswordConfirmLabelForeground = changed ? Brushes.LightSkyBlue : Brushes.Transparent;
                        IsBtnSaveVisible = changed && !string.IsNullOrEmpty(data.Item5);
                    });
        }

        public override void Navigate()
        {
            if (AppContext.Current.User is null)
            {
                NavigateTo<AuthViewModel>();
            }
            else
            {
                base.Navigate();
            }
        }

        public override async Task RefreshAsync()
        {
            if (AppContext.Current.User is null)
            {
                NavigateTo<AuthViewModel>();
            }
            
            var result = await AccountService.GetUserDataAsync();
            if (result.Ok)
                await AppContext.SetUserAsync(result.Data);

            _Refresh();
        }

        private void _Refresh()
        {
            FirstName = AppContext.Current.User!.FirstName;
            LastName = AppContext.Current.User!.LastName;
            Login = AppContext.Current.User!.Login;
            Password = "";
            PasswordConfirm = "";
        }

        private async Task _SaveAsync()
        {
            using var preloader = MainWindow.Current!.StartPreloader();
            
            var data = new UserPatchData
            {
                FirstName = FirstName ?? "",
                LastName = LastName ?? "",
                Login = string.IsNullOrEmpty(Login?.Trim()) ? null : Login.Trim(),
                Password = string.IsNullOrEmpty(Password) ? null : Password,
                PasswordConfirm = PasswordConfirm ?? "",
            };

            var result = await AccountService.UpdateUserDataAsync(data);

            if (result.Ok) await RefreshAsync();
        }

        private async Task _SignOutAsync()
        {
            using var preloader = MainWindow.Current!.StartPreloader();
            
            await AuthService.SignOutAsync();
            NavigateTo<AuthViewModel>();
        }
    }
}