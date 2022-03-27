namespace PassMeta.DesktopApp.Ui.Views.Base
{
    using DesktopApp.Ui.ViewModels.Base;
    using System;
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

        /// <summary>
        /// Navigate to a page by ViewModel.
        /// </summary>
        protected void NavigateTo<TNewViewModel>()
            where TNewViewModel : PageViewModel
        {
            var vm = (TNewViewModel) Activator.CreateInstance(typeof(TNewViewModel), DataContext!.HostScreen)!;
            vm.Navigate();
        }
    }
}