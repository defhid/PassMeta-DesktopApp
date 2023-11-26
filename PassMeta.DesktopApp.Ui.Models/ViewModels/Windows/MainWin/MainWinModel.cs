using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Internal;
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
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(isLoading => PreloaderEnabled = isLoading)
                .DisposeWith(disposables);

            MainPane.Activator.Activate().DisposeWith(disposables);
        });
    }

    /// <inheritdoc />
    public ViewModelActivator Activator { get; } = new();

    /// <inheritdoc />
    public RoutingState Router { get; } = new();

    public AppMode Mode { get; }

    public MainPane MainPane { get; }

    public ContentControl[] RightBarButtons => _rightBarButtons?.Value ?? Array.Empty<ContentControl>();

    public ICommand RefreshCurrentPageCommand => ReactiveCommand.CreateFromTask(RefreshCurrentPageAsync);

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