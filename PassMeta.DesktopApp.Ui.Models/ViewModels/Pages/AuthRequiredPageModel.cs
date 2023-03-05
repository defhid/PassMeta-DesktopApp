using System.Threading.Tasks;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages;

public class AuthRequiredPageModel : PageViewModel
{
    public AuthRequiredPageModel(IScreen hostScreen) : base(hostScreen)
    {
    }

    /// <inheritdoc />
    public override Task RefreshAsync() => Task.CompletedTask;
}