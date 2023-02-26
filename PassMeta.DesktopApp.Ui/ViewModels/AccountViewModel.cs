using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;

using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Models.Dto.Request;

using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Ui.App;
using PassMeta.DesktopApp.Ui.ViewModels.Base;
using Splat;

namespace PassMeta.DesktopApp.Ui.ViewModels;

public class AccountViewModel : PageViewModel
{
    private static IAccountService AccountService => Locator.Current.Resolve<IAccountService>();
    private static IAuthService AuthService => Locator.Current.Resolve<IAuthService>();
    private static IAppContextProvider AppContext => Locator.Current.Resolve<IAppContextProvider>();

    public override ContentControl[] RightBarButtons => new ContentControl[]
    {
        new Button
        {
            Opacity = 0.8,
            Content = "\uF3B1",
            Command = SignOutCommand,
            [ToolTip.TipProperty] = Resources.ACCOUNT__TOOLTIP_SIGN_OUT,
            [ToolTip.PlacementProperty] = PlacementMode.Left
        },
        new Button
        {
            Opacity = 0.8,
            Content = "\uE77A",
            Command = ResetSessionsCommand,
            [ToolTip.TipProperty] = Resources.ACCOUNT__TOOLTIP_RESET_SESSIONS,
            [ToolTip.PlacementProperty] = PlacementMode.Left
        }
    };

    private string? _fullName;
    public string? FullName
    {
        get => _fullName;
        set => this.RaiseAndSetIfChanged(ref _fullName, value);
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

    public ICommand ResetSessionsCommand => ReactiveCommand.CreateFromTask(_ResetSessionsAsync);

    public ICommand SaveCommand => ReactiveCommand.CreateFromTask(_SaveAsync);
        
    public AccountViewModel(IScreen hostScreen) : base(hostScreen)
    {
        if (AppContext.Current.User is null) return;
            
        _Refresh();
        this.WhenAnyValue(
                vm => vm.FullName,
                vm => vm.Login,
                vm => vm.Password,
                vm => vm.PasswordConfirm)
            .Subscribe(data =>
            {
                var user = AppContext.Current.User;

                var passwordConfirmNeed = data.Item2 != user.Login || !string.IsNullOrEmpty(data.Item3);
                var changed = passwordConfirmNeed || data.Item1 != user.FullName;

                IsPasswordConfirmVisible = passwordConfirmNeed;
                IsBtnSaveVisible = changed && (!passwordConfirmNeed || !string.IsNullOrEmpty(data.Item4));
            });
    }

    public override void TryNavigate()
    {
        if (AppContext.Current.User is null)
        {
            TryNavigateTo<AuthViewModel>();
        }
        else
        {
            base.TryNavigate();
        }
    }

    public override async Task RefreshAsync()
    {
        if (AppContext.Current.User is null)
        {
            TryNavigateTo<AuthViewModel>();
        }

        await AccountService.RefreshUserDataAsync();

        _Refresh();
    }

    private void _Refresh()
    {
        FullName = AppContext.Current.User?.FullName;
        Login = AppContext.Current.User?.Login;
        Password = "";
        PasswordConfirm = "";
    }

    private async Task _SaveAsync()
    {
        using var preloader = AppLoading.General.Begin();
            
        var data = new UserPatchData
        {
            FullName = FullName?.Trim() ?? "",
            Login = Login?.Trim() ?? "",
            Password = string.IsNullOrEmpty(Password) ? null : Password,
            PasswordConfirm = PasswordConfirm ?? "",
        };

        var result = await AccountService.UpdateUserDataAsync(data);

        if (result.Ok) await RefreshAsync();
    }

    private async Task _SignOutAsync()
    {
        using var preloader = AppLoading.General.Begin();
            
        await AuthService.SignOutAsync();
        TryNavigateTo<AuthViewModel>();
    }
        
    private static async Task _ResetSessionsAsync()
    {
        using var preloader = AppLoading.General.Begin();

        await AuthService.ResetAllExceptMeAsync();
    }
}