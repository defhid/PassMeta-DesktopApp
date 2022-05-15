namespace PassMeta.DesktopApp.Ui.Views
{
    using DesktopApp.Ui.ViewModels;
    using DesktopApp.Ui.Views.Base;
    
    using Avalonia.Markup.Xaml;
    
    public class SettingsView : PageView<SettingsViewModel>
    {
        public SettingsView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}