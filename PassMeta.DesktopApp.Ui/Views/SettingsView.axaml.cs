namespace PassMeta.DesktopApp.Ui.Views
{
    using ViewModels;
    using Base;
    
    using Avalonia.Markup.Xaml;
    
    public class SettingsView : PageView<SettingsViewModel>
    {
        public SettingsView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}