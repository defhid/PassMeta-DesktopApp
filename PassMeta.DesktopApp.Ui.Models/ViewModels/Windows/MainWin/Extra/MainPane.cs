using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Ui.Models.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.Account;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.JournalPage;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.LogsPage;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin.Extra;

/// <summary>
/// Main pane.
/// </summary>
public sealed class MainPane : ReactiveObject, IActivatableViewModel
{
    private readonly BehaviorSubject<bool> _devModeSource = new(false);
    private readonly BehaviorSubject<bool> _authSource = new(false);
    private readonly BehaviorSubject<MainPaneButtonModel?> _activeBtnSource = new(null);
    private readonly Dictionary<Func<PageViewModel>, PageViewModel> _pagesCache = new();
    private bool _isOpened;

    public MainPane(IScreen hostScreen)
    {
        var userContext = Locator.Current.Resolve<IUserContextProvider>();
        var appConfig = Locator.Current.Resolve<IAppConfigProvider>();

        this.WhenActivated(disposables =>
        {
            userContext.CurrentObservable
                .Subscribe(x => _authSource.OnNext(x.UserId is not null))
                .DisposeWith(disposables);

            appConfig.CurrentObservable
                .Subscribe(x => _devModeSource.OnNext(x.DevMode))
                .DisposeWith(disposables);

            hostScreen.Router.CurrentViewModel
                .Subscribe(vm => _activeBtnSource.OnNext(vm switch
                {
                    AuthPageModel => Account,
                    AccountPageModel => Account,
                    PwdStorageModel => Storage,
                    GeneratorPageModel => Generator,
                    JournalPageModel => Journal,
                    LogsPageModel => Logs,
                    SettingsPageModel => Settings,
                    _ => null
                }))
                .DisposeWith(disposables);

            userContext.CurrentObservable
                .Subscribe(_ => _pagesCache.Clear())
                .DisposeWith(disposables);

            appConfig.CurrentObservable
                .Subscribe(_ => _pagesCache.Clear())
                .DisposeWith(disposables);
        });

        var createButton = BuildButtonFactory();

        Account = createButton(
            Resources.APP__MENU_BTN__ACCOUNT,
            "\uE77b",
            Observable.Return(true),
            () => userContext.Current.UserId is null
                ? new AuthPageModel(hostScreen)
                : new AccountPageModel(hostScreen));

        Storage = createButton(
            Resources.APP__MENU_BTN__STORAGE,
            "\uE8F1",
            _authSource,
            () => new PwdStorageModel(hostScreen, HostWindowProvider!));

        Generator = createButton(
            Resources.APP__MENU_BTN__GENERATOR,
            "\uEA80",
            Observable.Return(true),
            () => new GeneratorPageModel(hostScreen));

        Journal = createButton(
            Resources.APP__MENU_BTN__JOURNAL,
            "\uE823",
            _authSource,
            () => new JournalPageModel(hostScreen));

        Logs = createButton(
            Resources.APP__MENU_BTN__LOGS,
            "\uE9D9",
            _devModeSource,
            () => new LogsPageModel(hostScreen));

        Settings = createButton(
            Resources.APP__MENU_BTN__SETTINGS,
            "\uE713",
            Observable.Return(true),
            () => new SettingsPageModel(hostScreen));
    }

    /// <inheritdoc cref="HostWindowProvider"/>
    public HostWindowProvider? HostWindowProvider { private get; set; }

    public bool IsOpened
    {
        get => _isOpened;
        set => this.RaiseAndSetIfChanged(ref _isOpened, value);
    }

    /// <inheritdoc />
    public ViewModelActivator Activator { get; } = new();

    public MainPaneButtonModel Account { get; }

    public MainPaneButtonModel Storage { get; }

    public MainPaneButtonModel Generator { get; }

    public MainPaneButtonModel Journal { get; }

    public MainPaneButtonModel Logs { get; }

    public MainPaneButtonModel Settings { get; }

    private MainPaneButtonModelFactory BuildButtonFactory()
    {
        var shortMode = this.WhenAnyValue(pane => pane.IsOpened).Select(isOpened => !isOpened);

        return (text, icon, isVisible, pageFactory) =>
        {
            var btn = new MainPaneButtonModel(text, icon, shortMode)
            {
                IsVisible = isVisible,
                Command = ReactiveCommand.CreateFromTask(async () =>
                {
                    if (!_pagesCache.TryGetValue(pageFactory, out var pvm))
                    {
                        pvm =  pageFactory();
                        _pagesCache[pageFactory] = pvm;
                    }

                    await pvm.TryNavigateAsync();
                    IsOpened = false;
                })
            };
            btn.IsActive = _activeBtnSource.Select(x => ReferenceEquals(x, btn));
            return btn;
        };
    }

    private delegate MainPaneButtonModel MainPaneButtonModelFactory(
        string text,
        string icon,
        IObservable<bool> isVisible,
        Func<PageViewModel> pageFactory);
}