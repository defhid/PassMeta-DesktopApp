using System;
using System.Reactive;
using Avalonia.Media;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.ViewModels.Base;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class AccountViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/account";

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
        
        public AccountViewModel(IScreen hostScreen) : base(hostScreen)
        {
            if (AppConfig.Current.User is null) return;
            
            Refresh();
            this.WhenAnyValue(
                    vm => vm.FirstName,
                    vm => vm.LastName,
                    vm => vm.Login,
                    vm => vm.Password,
                    vm => vm.PasswordConfirm)
                .Subscribe(data =>
                    {
                        var user = AppConfig.Current.User;
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
            if (AppConfig.Current.User is null)
            {
                NavigateTo<AuthViewModel>();
            }
            else
            {
                base.Navigate();
            }
        }

        public void Refresh()
        {
            FirstName = AppConfig.Current.User!.FirstName;
            LastName = AppConfig.Current.User!.LastName;
            Login = AppConfig.Current.User!.Login;
            Password = "";
            PasswordConfirm = "";
        }
    }
}