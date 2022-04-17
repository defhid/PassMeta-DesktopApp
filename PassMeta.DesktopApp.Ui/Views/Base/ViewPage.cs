namespace PassMeta.DesktopApp.Ui.Views.Base
{
    using DesktopApp.Ui.ViewModels.Base;
    using Avalonia.ReactiveUI;
    
    public abstract class ViewPage<TViewModel> : ReactiveUserControl<TViewModel>
        where TViewModel : PageViewModel
    {
        /// <inheritdoc cref="ReactiveUserControl{TViewModel}.DataContext"/>
        protected new TViewModel? DataContext
        {
            get => (TViewModel)base.DataContext!;
            set => base.DataContext = value;
        }
    }
}