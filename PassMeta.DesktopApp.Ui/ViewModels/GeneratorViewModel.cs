using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class GeneratorViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/generator";

        public GeneratorViewModel(IScreen hostScreen) : base(hostScreen)
        {
        }
    }
}