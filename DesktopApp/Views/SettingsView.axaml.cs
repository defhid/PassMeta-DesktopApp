using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DesktopApp.ViewModels;

namespace DesktopApp.Views
{
    public class SettingsView : ReactiveUserControl<SettingsViewModel>
    {
        public SettingsView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}