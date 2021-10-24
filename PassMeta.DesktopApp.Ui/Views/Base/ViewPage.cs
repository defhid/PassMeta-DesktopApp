using System;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.ViewModels.Base;

namespace PassMeta.DesktopApp.Ui.Views.Base
{
    public abstract class ViewPage<TViewModel> : ReactiveUserControl<TViewModel>
        where TViewModel : ViewModelPage
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
            where TNewViewModel : ViewModelPage
        {
            var vm = (TNewViewModel) Activator.CreateInstance(typeof(TNewViewModel), DataContext!.HostScreen)!;
            vm.Navigate();
        }
    }
}