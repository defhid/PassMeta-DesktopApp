using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin.Extra;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin;

/// <summary>
/// Main window ViewModel.
/// </summary>
public sealed class MainWinModel : ReactiveObject, IScreen, IActivatableViewModel
{
    private ObservableAsPropertyHelper<ContentControl[]?>? _rightBarButtons;
    private bool _preloaderEnabled = true;
    
    /// <summary></summary>
    public MainWinModel()
    {
        Activator = new ViewModelActivator();
        Router = new RoutingState();
        MainPane = new MainPane(this);
        Mode = new AppMode();

        this.WhenActivated(disposables =>
        {
            _rightBarButtons = Router.CurrentViewModel
                .Select(x => x is PageViewModel pvm ? pvm.RightBarButtons : null)
                .ToProperty(this, nameof(RightBarButtons))
                .DisposeWith(disposables);
        });
        
        // TODO
        
        d(ViewModel!.Router.CurrentViewModel.Subscribe(HandleNavigate));
        
        private void HandleNavigate(IRoutableViewModel? viewModel)
        {
            var mainPaneButtons = ViewModel!.MainPane.Buttons;
            mainPaneButtons.CurrentActive = viewModel switch
            {
                AuthPageModel => mainPaneButtons.Account,
                AccountPageModel => mainPaneButtons.Account,
                StoragePageModel => mainPaneButtons.Storage,
                GeneratorPageModel => mainPaneButtons.Generator,
                JournalPageModel => mainPaneButtons.Journal,
                LogsPageModel => mainPaneButtons.Logs,
                SettingsPageModel => mainPaneButtons.Settings,
                _ => mainPaneButtons.CurrentActive
            };
            ViewModel.MainPane.IsOpened = false;
        }
        
        private static void MenuBtnClick(object? sender, Action action)
        {
            var btn = (Button)sender!;
            if (!btn.Classes.Contains("active"))
            {
                action();
            }
        }
    }

    /// <inheritdoc />
    public ViewModelActivator Activator { get; }
    
    /// <inheritdoc />
    public RoutingState Router { get; }

    public MainPane MainPane { get; }

    public AppMode Mode { get; }

    public ContentControl[] RightBarButtons => _rightBarButtons?.Value ?? Array.Empty<ContentControl>();

    public bool PreloaderEnabled
    {
        get => _preloaderEnabled;
        set => this.RaiseAndSetIfChanged(ref _preloaderEnabled, value);
    }
}