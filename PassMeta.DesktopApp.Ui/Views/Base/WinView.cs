using Avalonia.ReactiveUI;
using Avalonia;

namespace PassMeta.DesktopApp.Ui.Views.Base;

public abstract class WinView<TViewModel> : ReactiveWindow<TViewModel>
    where TViewModel : class, new()
{
    protected WinView()
    {
        ViewModel = new TViewModel(); // TODO: need?

#if DEBUG
        this.AttachDevTools();
#endif
    }
}