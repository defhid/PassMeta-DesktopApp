using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.App;
using PassMeta.DesktopApp.Ui.Models.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin.Extra;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin;

/// <summary>
/// Main window ViewModel.
/// </summary>
public sealed class MainWinModel : ReactiveObject, IScreen, IActivatableViewModel
{
    private readonly AppLoading _appLoading = Locator.Current.Resolve<AppLoading>();
    private readonly BehaviorSubject<bool> _isOnlineSource = new(false);

    private ObservableAsPropertyHelper<ContentControl[]?>? _rightBarButtons;
    private bool _preloaderEnabled = true;

    /// <summary></summary>
    public MainWinModel()
    {
        Mode = new AppMode(_isOnlineSource);
        MainPane = new MainPane(this);

        this.WhenActivated(disposables =>
        {
            _rightBarButtons = Router.CurrentViewModel
                .Select(x => x is PageViewModel pvm ? pvm.RightBarButtons : null)
                .ToProperty(this, nameof(RightBarButtons))
                .DisposeWith(disposables);

            Locator.Current.Resolve<IPassMetaClient>()
                .OnlineObservable
                .Subscribe(_isOnlineSource.OnNext)
                .DisposeWith(disposables);

            _appLoading.General.ActiveObservable
                .Subscribe(isLoading => Dispatcher.UIThread.InvokeAsync(
                    () => PreloaderEnabled = isLoading,
                    isLoading ? DispatcherPriority.MaxValue : DispatcherPriority.Normal))
                .DisposeWith(disposables);

            MainPane.Activator.Activate().DisposeWith(disposables);
        });
    }

    /// <inheritdoc cref="HostWindowProvider"/>
    public HostWindowProvider? HostWindowProvider
    {
        set => MainPane.HostWindowProvider = value;
    }

    /// <inheritdoc />
    public ViewModelActivator Activator { get; } = new();

    /// <inheritdoc />
    public RoutingState Router { get; } = new();

    /// <summary></summary>
    public AppMode Mode { get; }

    /// <summary></summary>
    public MainPane MainPane { get; }

    /// <summary></summary>
    public ContentControl[] RightBarButtons => _rightBarButtons?.Value ?? Array.Empty<ContentControl>();

    /// <summary></summary>
    public ICommand RefreshCurrentPageCommand => ReactiveCommand.CreateFromTask(RefreshCurrentPageAsync);

    /// <summary></summary>
    public bool PreloaderEnabled
    {
        get => _preloaderEnabled;
        set => this.RaiseAndSetIfChanged(ref _preloaderEnabled, value);
    }

    private async Task RefreshCurrentPageAsync()
    {
        using var preloader = _appLoading.General.Begin();

        await Locator.Current.Resolve<IPassMetaClient>().CheckConnectionAsync();

        if (await Router.CurrentViewModel.FirstAsync() is PageViewModel pvm)
        {
            await pvm.RefreshAsync();
        }
    }
}