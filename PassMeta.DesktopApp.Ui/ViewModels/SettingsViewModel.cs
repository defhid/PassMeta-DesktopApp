namespace PassMeta.DesktopApp.Ui.ViewModels
{
    using DesktopApp.Core.Utils;
    using DesktopApp.Ui.ViewModels.Base;
    
    using System.Threading.Tasks;
    using ReactiveUI;

    public class SettingsViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/settings";

        public string[][] Lang { get; } = AppConfig.AppCultures;

        public string? ServerUrl { get; set; }

        public int CultureIndex { get; set; }

        public SettingsViewModel(IScreen hostScreen) : base(hostScreen)
        {
            ServerUrl = AppConfig.Current.ServerUrl == string.Empty
                ? "https://" 
                : AppConfig.Current.ServerUrl;
            
            for (var i = 0; i < Lang.Length; ++i)
            {
                if (Lang[i][1] != AppConfig.Current.CultureCode) continue;
                CultureIndex = i;
                break;
            }
        }

        public override Task RefreshAsync()
        {
            _Refresh();
            return Task.CompletedTask;
        }

        private void _Refresh()
        {
            ServerUrl = AppConfig.Current.ServerUrl == string.Empty
                ? "https://" 
                : AppConfig.Current.ServerUrl;
            
            for (var i = 0; i < Lang.Length; ++i)
            {
                if (Lang[i][1] != AppConfig.Current.CultureCode) continue;
                CultureIndex = i;
                break;
            }
        }
    }
}