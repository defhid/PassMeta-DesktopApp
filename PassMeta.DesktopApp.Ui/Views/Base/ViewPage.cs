using System;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.ViewModels.Base;

namespace PassMeta.DesktopApp.Ui.Views.Base
{
    public interface IViewPage
    {
        /// <summary>
        /// Refresh page.
        /// </summary>
        Task RefreshAsync();
        
        /// <summary>
        /// Navigate to a page by ViewModel.
        /// </summary>
        void NavigateTo<TNewViewModel>() 
            where TNewViewModel : ViewModelPage;
    }
    
    public abstract class ViewPage<TViewModel> : ReactiveUserControl<TViewModel>, IViewPage
        where TViewModel : ViewModelPage
    {
        /// <inheritdoc cref="ReactiveUserControl{TViewModel}.DataContext"/>
        protected new TViewModel? DataContext
        {
            get => (TViewModel)base.DataContext!;
            set => base.DataContext = value;
        }

        /// <inheritdoc />
        public abstract Task RefreshAsync();

        /// <inheritdoc />
        public void NavigateTo<TNewViewModel>()
            where TNewViewModel : ViewModelPage
        {
            var vm = (TNewViewModel) Activator.CreateInstance(typeof(TNewViewModel), DataContext!.HostScreen)!;
            vm.Navigate();
        }
    }
}