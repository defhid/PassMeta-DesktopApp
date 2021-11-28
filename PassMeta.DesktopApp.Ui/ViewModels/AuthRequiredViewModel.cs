namespace PassMeta.DesktopApp.Ui.ViewModels
{
    using DesktopApp.Core.Utils;
    using DesktopApp.Ui.ViewModels.Base;
    
    using System;
    using System.Threading.Tasks;
    using ReactiveUI;
    
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
            if (AppConfig.Current.User is not null)
            {
                NavigateTo(ForViewModelPageType);
            }
            
            return Task.CompletedTask;
        }
    }
}