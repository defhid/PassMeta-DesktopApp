namespace PassMeta.DesktopApp.Ui.Views
{
    using DesktopApp.Ui.ViewModels;
    using DesktopApp.Ui.Views.Base;
    
    using Avalonia.Markup.Xaml;
    
    public class SettingsView : ViewPage<SettingsViewModel>
    {
        public SettingsView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}