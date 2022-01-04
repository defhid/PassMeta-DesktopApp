namespace PassMeta.DesktopApp.Ui.ViewModels
{
    using DesktopApp.Ui.ViewModels.Base;
    
    using System;
    using System.Threading.Tasks;
    using ReactiveUI;
    
    using AppContext = Core.Utils.AppContext;

    public class AuthRequiredViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/auth-required";
        
        /// <summary><see cref="ViewModelPage"/></summary>
        public Type ForViewModelPageType { get; }

        public AuthRequiredViewModel(IScreen hostScreen, Type forPage) : base(hostScreen)
        {
            ForViewModelPageType = forPage;
        }

        public override Task RefreshAsync()
        {
            if (AppContext.Current.User is not null)
            {
                NavigateTo(ForViewModelPageType);
            }
            
            return Task.CompletedTask;
        }
    }
}