namespace PassMeta.DesktopApp.Ui.ViewModels.Base
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Avalonia.Controls;
    using ReactiveUI;
    
    public abstract class PageViewModel : ReactiveObject, IRoutableViewModel
    {
        public static event Action<PageViewModel>? OnNavigated;

        public string? UrlPathSegment { get; }
        
        public IScreen HostScreen { get; }

        public virtual ContentControl[] RightBarButtons { get; } = Array.Empty<ContentControl>();

        protected PageViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;
            UrlPathSegment = GetType().Name;
        }
        
        protected void NavigateTo<TViewModel>(params object?[]? args) 
            where TViewModel : PageViewModel
        {
            args ??= Array.Empty<object>();
            var vm = (TViewModel)Activator.CreateInstance(typeof(TViewModel), new object[] {HostScreen}.Concat(args).ToArray())!;
            vm.Navigate();
        }
        
        protected void NavigateTo(Type viewModelType)
        {
            var vm = (PageViewModel)Activator.CreateInstance(viewModelType, HostScreen)!;
            vm.Navigate();
        }

        /// <summary>
        /// Refresh page.
        /// </summary>
        /// <remarks>Invokes with preloader.</remarks>
        public abstract Task RefreshAsync();

        public virtual void Navigate()
        {
            HostScreen.Router.Navigate.Execute(this);
            OnNavigated?.Invoke(this);
        }

        protected void FakeNavigated()
        {
             OnNavigated?.Invoke(this);
        }
    }
}