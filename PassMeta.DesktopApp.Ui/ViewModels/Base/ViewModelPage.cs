using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels.Base
{
    public abstract class ViewModelPage : ReactiveObject, IRoutableViewModel
    {
        public static event Action<ViewModelPage>? OnNavigated;

        public abstract string? UrlPathSegment { get; }
        
        public IScreen HostScreen { get; }

        public virtual ContentControl[] RightBarButtons { get; } = Array.Empty<ContentControl>();

        protected ViewModelPage(IScreen hostScreen)
        {
            HostScreen = hostScreen;
        }
        
        protected void NavigateTo<TViewModel>(params object?[]? args) 
            where TViewModel : ViewModelPage
        {
            args ??= Array.Empty<object>();
            var vm = (TViewModel)Activator.CreateInstance(typeof(TViewModel), new object[] {HostScreen}.Concat(args).ToArray())!;
            vm.Navigate();
        }
        
        protected void NavigateTo(Type viewModelType)
        {
            var vm = (ViewModelPage)Activator.CreateInstance(viewModelType, HostScreen)!;
            vm.Navigate();
        }

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