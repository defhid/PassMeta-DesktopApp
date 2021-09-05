using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.ViewModels.Base;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class StorageViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/storage";

        public StorageViewModel(IScreen hostScreen) : base(hostScreen)
        {
        }

        public override void Navigate()
        {
            if (AppConfig.Current.User is null)
            {
                FakeNavigated();
                NavigateTo<AuthRequiredViewModel>();
            }
            else
            {
                base.Navigate();
            }
        }
    }
}