using System;
using System.Reactive.Linq;
using Avalonia.Controls;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin.Extra;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin;

public sealed class MainWinModel : ReactiveObject, IScreen, IDisposable
{
    public MainWinModel()
    {
        Router = new RoutingState();
        MainPane = new MainPane(this);
        RightBarButtons = Router.CurrentViewModel.Select(x => x is PageViewModel pvm 
            ? pvm.RightBarButtons
            : Array.Empty<ContentControl>());
    }
    
    public RoutingState Router { get; }

    public MainPane MainPane { get; }

    public AppMode Mode { get; } = new();

    public IObservable<ContentControl[]> RightBarButtons { get; }
        
    private bool _preloaderEnabled = true;
    public bool PreloaderEnabled
    {
        get => _preloaderEnabled;
        set => this.RaiseAndSetIfChanged(ref _preloaderEnabled, value);
    }

    public void Dispose()
    {
        MainPane.Dispose();
    }
}