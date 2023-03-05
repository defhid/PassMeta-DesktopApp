using System.Threading.Tasks;
using PassMeta.DesktopApp.Ui.Models.Base;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models;

public class AuthRequiredModel : PageViewModel
{
    public AuthRequiredModel(IScreen hostScreen) : base(hostScreen)
    {
    }

    /// <inheritdoc />
    public override Task RefreshAsync() => Task.CompletedTask;
}