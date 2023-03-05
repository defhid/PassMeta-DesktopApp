using System.Reactive;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Ui.Models;
using PassMeta.DesktopApp.Ui.Models.Etc;
using PassMeta.DesktopApp.Ui.Views.Etc;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Views;

public class SettingsView : ReactiveUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        AvaloniaXamlLoader.Load(this);

        this.WhenActivated(d => d(ViewModel!.ShowInfo.RegisterHandler(ShowInfoAsync)));
    }

    private async Task ShowInfoAsync(InteractionContext<ApplicationInfoViewModel, Unit> interaction)
    {
        var window = new ApplicationInfoWin { ViewModel = interaction.Input };
        
        await window.ShowDialog(App.App.MainWindow);
        
        interaction.SetOutput(Unit.Default);
    }
}