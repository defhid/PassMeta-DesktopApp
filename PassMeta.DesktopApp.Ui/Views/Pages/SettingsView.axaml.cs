using System.Reactive;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows;
using PassMeta.DesktopApp.Ui.Views.Windows;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Views.Pages;

public partial class SettingsView : ReactiveUserControl<SettingsPageModel>
{
    public SettingsView()
    {
        InitializeComponent();

        this.WhenActivated(d => d(ViewModel!.ShowInfo.RegisterHandler(ShowInfoAsync)));
    }

    private static async Task ShowInfoAsync(InteractionContext<AppInfoWinModel, Unit> interaction)
    {
        var window = new ApplicationInfoWindow { ViewModel = interaction.Input };
        
        await window.ShowDialog(App.App.MainWindow);
        
        interaction.SetOutput(Unit.Default);
    }
}