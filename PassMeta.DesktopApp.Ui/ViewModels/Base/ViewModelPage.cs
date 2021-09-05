using System;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels.Base
{
    public abstract class ViewModelPage : ReactiveObject, IRoutableViewModel
    {
        public abstract string? UrlPathSegment { get; }
        
        public IScreen HostScreen { get; }

        public static event Action<ViewModelPage>? OnNavigated;

        protected ViewModelPage(IScreen hostScreen)
        {
            HostScreen = hostScreen;
        }
        
        protected void NavigateTo<TViewModel>() 
            where TViewModel : ViewModelPage
        {
            var vm = (TViewModel) Activator.CreateInstance(typeof(TViewModel), HostScreen)!;
            vm.Navigate();
        }

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