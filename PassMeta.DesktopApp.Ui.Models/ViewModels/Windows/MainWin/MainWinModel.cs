using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Controls;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Extensions;
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

            MainPane.Activator.Activate().DisposeWith(disposables);
        });
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
    public bool PreloaderEnabled
    {
        get => _preloaderEnabled;
        set => this.RaiseAndSetIfChanged(ref _preloaderEnabled, value);
    }
}