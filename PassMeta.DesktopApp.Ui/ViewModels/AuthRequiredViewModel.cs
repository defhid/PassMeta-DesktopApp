using System;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.ViewModels.Base;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
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