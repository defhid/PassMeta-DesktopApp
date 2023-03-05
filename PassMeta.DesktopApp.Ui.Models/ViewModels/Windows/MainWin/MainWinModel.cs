using System;
using Avalonia.Controls;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin.Extra;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin;

public sealed class MainWinModel : ReactiveObject, IScreen, IDisposable
{
    public RoutingState Router { get; } = new();

    public MainPane MainPane { get; } = new();

    public AppMode Mode { get; } = new();

    private ContentControl[]? _rightBarButtons;
    public ContentControl[]? RightBarButtons
    {
        get => _rightBarButtons;
        set => this.RaiseAndSetIfChanged(ref _rightBarButtons, value);
    }
        
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