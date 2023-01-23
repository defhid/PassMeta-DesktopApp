namespace PassMeta.DesktopApp.Ui.ViewModels
{
    using Base;
    using System;
    using System.Threading.Tasks;
    using ReactiveUI;

    public class AuthRequiredViewModel : PageViewModel
    {
        /// <summary><see cref="PageViewModel"/></summary>
        public Type ForViewModelPageType { get; }

        public AuthRequiredViewModel(IScreen hostScreen, Type forPage) : base(hostScreen)
        {
            ForViewModelPageType = forPage;
        }

        public override Task RefreshAsync()
        {
            if (Core.AppContext.Current.User is not null)
            {
                TryNavigateTo(ForViewModelPageType);
            }
            
            return Task.CompletedTask;
        }
    }
}