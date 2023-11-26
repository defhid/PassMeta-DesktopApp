using System.Reactive;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows;
using PassMeta.DesktopApp.Ui.Views.Windows;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views.Pages;

public partial class SettingsView : ReactiveUserControl<SettingsPageModel>
{
    private readonly IHostWindowProvider _hostWindowProvider = Locator.Current.Resolve<IHostWindowProvider>();
    
    public SettingsView()
    {
        InitializeComponent();

        this.WhenActivated(d => d(ViewModel!.ShowInfo.RegisterHandler(ShowInfoAsync)));
    }

    private async Task ShowInfoAsync(IInteractionContext<AppInfoWinModel, Unit> interaction)
    {
        var window = new ApplicationInfoWindow { ViewModel = interaction.Input };
        
        await window.ShowDialog(_hostWindowProvider.Window);
        
        interaction.SetOutput(Unit.Default);
    }
}