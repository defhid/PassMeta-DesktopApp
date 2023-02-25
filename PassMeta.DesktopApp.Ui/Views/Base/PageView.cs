namespace PassMeta.DesktopApp.Ui.Views.Base;

using Avalonia.ReactiveUI;
using ReactiveUI;

public abstract class PageView<TViewModel> : ReactiveUserControl<TViewModel>
    where TViewModel : ReactiveObject
{
    /// <inheritdoc cref="ReactiveUserControl{TViewModel}.DataContext"/>
    protected new TViewModel? DataContext
    {
        get => (TViewModel)base.DataContext!;
        set => base.DataContext = value;
    }
}