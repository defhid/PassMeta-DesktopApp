using System;
using System.Reactive.Linq;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin.Extra;

public class MainPaneBtn : ReactiveObject
{
    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    private bool _isVisibl;
    public bool IsVisible
    {
        get => _isVisibl;
        set => this.RaiseAndSetIfChanged(ref _isVisibl, value);
    }

    public IObservable<string> Content { get; }

    public MainPaneBtn(string text, string icon, IObservable<bool> shortModeObservable)
    {
        Content = shortModeObservable.Select(isShort => isShort ? icon : text);
        IsVisible = true;
    }
}