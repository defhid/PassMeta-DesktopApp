using System.Reactive.Linq;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin.Extra;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Previews;

/// <inheritdoc />
public class MainPaneButtonModelPreview : MainPaneButtonModel
{
    public MainPaneButtonModelPreview() : base("Button", "P", Observable.Return(true))
    {
        IsActive = Observable.Return(false);
        IsVisible = Observable.Return(true);
        Command = ReactiveCommand.Create(() => { });
    }
}