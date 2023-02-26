using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
    
using Avalonia.Media;
using PassMeta.DesktopApp.Common.Extensions;
using ReactiveUI;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Extensions;

namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage.Components;

public class PassFileBtn : ReactiveObject
{
    private PwdPassFile? _passFile;
    public PwdPassFile? PassFile
    {
        get => _passFile;
        private set => this.RaiseAndSetIfChanged(ref _passFile, value);
    }

    public ISolidColorBrush StateColor { get; private set; }

    public IObservable<string> Name { get; }
         
    public IObservable<ISolidColorBrush?> Color { get; }

    public IObservable<double> Opacity { get; }

    public ReactCommand OpenCommand { get; }

    private readonly ObservableAsPropertyHelper<bool> _shortMode;
    private bool ShortMode => _shortMode.Value;

    public PassFileBtn(PwdPassFile passFile, IObservable<bool> shortModeObservable)
    {
        _passFile = passFile;
        _shortMode = shortModeObservable.ToProperty(this, nameof(ShortMode));
            
        StateColor = passFile.GetStateColor();
            
        Name = this.WhenAnyValue(btn => btn.PassFile, btn => btn.ShortMode)
            .Select(pair => pair.Item1 is null 
                ? "~"
                : pair.Item1.IsLocalDeleted()
                    ? '~' + (pair.Item2 ? pair.Item1.Name[..1] : pair.Item1.Name)
                    : pair.Item2 ? pair.Item1.Name[..2] : pair.Item1.Name);
            
        var passFileObservable = this.WhenAnyValue(btn => btn.PassFile);

        Color = passFileObservable.Select(pf => pf?.GetPassFileColor().Brush);

        Opacity = passFileObservable.Select(pf => pf?.IsLocalDeleted() ?? true ? 0.6d : 1d);

        OpenCommand = ReactiveCommand.CreateFromTask(OpenAsync, passFileObservable.Select(pf => pf is not null));

    }

    public async Task OpenAsync()
    {
        var win = new Views.Storage.PassFileWin(PassFile!);

        await win.ShowDialog(App.App.MainWindow);
            
        RefreshState();
    }

    public void RefreshState()
    {
        StateColor = PassFile.GetStateColor();
        this.RaisePropertyChanged(nameof(StateColor));
    }
}