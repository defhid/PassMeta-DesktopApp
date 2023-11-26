using System.Threading.Tasks;
using System.Windows.Input;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Dto.Request;
using PassMeta.DesktopApp.Common.Models.Internal;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.Account;

/// <summary>
/// Authorization page ViewModel.
/// </summary>
public class AuthPageModel : PageViewModel
{
    private readonly IUserContextProvider _userContext = Locator.Current.Resolve<IUserContextProvider>();
    private readonly IAuthService _authService = Locator.Current.Resolve<IAuthService>();
    private readonly AppLoading _appLoading = Locator.Current.Resolve<AppLoading>();

    public AuthPageModel(IScreen hostScreen) : base(hostScreen)
    {
    }

    public string? Login { get; set; }

    public string? Password { get; set; }

    /// <summary>
    /// Log in to an existing account.
    /// </summary>
    public ICommand LogInCommand => ReactiveCommand.CreateFromTask(LogInAsync);

    /// <summary>
    /// Register a new account.
    /// </summary>
    public ICommand RegisterCommand => ReactiveCommand.CreateFromTask(RegisterAsync);

    /// <inheritdoc />
    public override async ValueTask RefreshAsync()
    {
        if (_userContext.Current.UserId is not null)
        {
            await new AccountPageModel(HostScreen).TryNavigateAsync();
            return;
        }

        Login = "";
        Password = "";

        this.RaisePropertyChanged(nameof(Login));
        this.RaisePropertyChanged(nameof(Password));
    }

    private async Task LogInAsync()
    {
        using var loading = _appLoading.General.Begin();

        var data = new SignInPostData(Login?.Trim() ?? "", Password ?? "");

        var result = await _authService.LogInAsync(data);
        if (result.Ok)
        {
            await RefreshAsync();
        }
    }

    private async Task RegisterAsync()
    {
        using var loading = _appLoading.General.Begin();

        var data = new SignUpPostData(Login?.Trim() ?? "", Password ?? "", "Unknown");

        var result = await _authService.RegisterAsync(data);
        if (result.Ok)
        {
            await RefreshAsync();
        }
    }
}