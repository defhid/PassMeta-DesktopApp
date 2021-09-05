using PassMeta.DesktopApp.Core;
using PassMeta.DesktopApp.Core.Utils;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class SettingsViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/settings";

        public string Title => Resources.SETTINGS__TITLE;

        public string SaveBtnContent => Resources.SETTINGS__SAVE_BTN;

        public string LocalizationLabel => Resources.SETTINGS__LOCALIZATION;

        public string[][] Lang { get; } = AppConfig.Cultures;

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
    }
}