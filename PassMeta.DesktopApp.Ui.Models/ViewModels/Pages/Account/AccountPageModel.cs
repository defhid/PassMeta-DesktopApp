using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.App;
using PassMeta.DesktopApp.Common.Models.Dto.Request;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.Account;

/// <summary>
/// Account page ViewModel.
/// </summary>
public class AccountPageModel : PageViewModel
{
    private readonly IAppContextProvider _appContext = Locator.Current.Resolve<IAppContextProvider>();
    private readonly IAccountService _accountService = Locator.Current.Resolve<IAccountService>();
    private readonly IAuthService _authService = Locator.Current.Resolve<IAuthService>();
    private readonly AppLoading _appLoading = Locator.Current.Resolve<AppLoading>();

    private string? _fullName;
    private string? _login;
    private string? _password;
    private string? _passwordConfirm;
    private bool _isPasswordConfirmVisible;
    private bool _isBtnSaveVisible;

    public AccountPageModel(IScreen hostScreen) : base(hostScreen) => 
        this.WhenActivated(disposables => 
        {
            _appContext.CurrentObservable
                .Subscribe(_ => RefreshFields())
                .DisposeWith(disposables);

            this.WhenAnyValue(
                    vm => vm.FullName,
                    vm => vm.Login,
                    vm => vm.Password,
                    vm => vm.PasswordConfirm)
                .Subscribe(data =>
                {
                    var user = _appContext.Current.User;
                    if (user is null)
                    {
                        return;
                    }

                    var passwordConfirmNeed = data.Item2 != user.Login || !string.IsNullOrEmpty(data.Item3);
                    var changed = passwordConfirmNeed || data.Item1 != user.FullName;

                    IsPasswordConfirmVisible = passwordConfirmNeed;
                    IsBtnSaveVisible = changed && (!passwordConfirmNeed || !string.IsNullOrEmpty(data.Item4));
                })
                .DisposeWith(disposables);
        });

    /// <inheritdoc />
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

    public string? FullName
    {
        get => _fullName;
        set => this.RaiseAndSetIfChanged(ref _fullName, value);
    }

    public string? Login
    {
        get => _login;
        set => this.RaiseAndSetIfChanged(ref _login, value);
    }

    public string? Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    public string? PasswordConfirm
    {
        get => _passwordConfirm;
        set => this.RaiseAndSetIfChanged(ref _passwordConfirm, value);
    }

    public bool IsPasswordConfirmVisible
    {
        get => _isPasswordConfirmVisible;
        set => this.RaiseAndSetIfChanged(ref _isPasswordConfirmVisible, value);
    }

    public bool IsBtnSaveVisible
    {
        get => _isBtnSaveVisible;
        private set => this.RaiseAndSetIfChanged(ref _isBtnSaveVisible, value);
    }

    /// <summary>
    /// Reset current session.
    /// </summary>
    private ICommand SignOutCommand => ReactiveCommand.CreateFromTask(SignOutAsync);

    /// <summary>
    /// Reset all user sessions except current.
    /// </summary>
    private ICommand ResetSessionsCommand => ReactiveCommand.CreateFromTask(ResetSessionsAsync);

    /// <summary>
    /// Save changed user info.
    /// </summary>
    public ICommand SaveCommand => ReactiveCommand.CreateFromTask(SaveAsync);

    /// <inheritdoc />
    public override async ValueTask RefreshAsync()
    {
        if (_appContext.Current.User is null)
        {
            await new AuthPageModel(HostScreen).TryNavigateAsync();
            return;
        }

        await _accountService.RefreshUserDataAsync();

        RefreshFields();
    }

    private void RefreshFields()
    {
        FullName = _appContext.Current.User?.FullName;
        Login = _appContext.Current.User?.Login;
        Password = "";
        PasswordConfirm = "";
    }

    private async Task SaveAsync()
    {
        using var preloader = _appLoading.General.Begin();

        var data = new UserPatchData
        {
            FullName = FullName?.Trim() ?? "",
            Login = Login?.Trim() ?? "",
            Password = string.IsNullOrEmpty(Password) ? null : Password,
            PasswordConfirm = PasswordConfirm ?? "",
        };

        var result = await _accountService.UpdateUserDataAsync(data);
        if (result.Ok)
        {
            await RefreshAsync();
        }
    }

    private async Task SignOutAsync()
    {
        using var preloader = _appLoading.General.Begin();

        await _authService.LogOutAsync();

        await RefreshAsync();
    }

    private async Task ResetSessionsAsync()
    {
        using var preloader = _appLoading.General.Begin();

        await _authService.ResetAllExceptMeAsync();
        
        await RefreshAsync();
    }
}