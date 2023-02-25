namespace PassMeta.DesktopApp.Ui.ViewModels;

using Base;
using System.Threading.Tasks;
using ReactiveUI;

public class AuthRequiredViewModel : PageViewModel
{
    public AuthRequiredViewModel(IScreen hostScreen) : base(hostScreen)
    {
    }

    /// <inheritdoc />
    public override Task RefreshAsync() => Task.CompletedTask;
}