using System;
using System.Reactive.Linq;
using System.Windows.Input;
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
    
    public ICommand Command { get; }

    public IObservable<string> Content { get; }

    public MainPaneBtn(string text, string icon, IObservable<bool> shortModeObservable, ICommand command)
    {
        Content = shortModeObservable.Select(isShort => isShort ? icon : text);
        IsVisible = true;
        Command = command;
    }
}