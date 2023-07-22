using System;
using System.Reactive;
using Avalonia.Media;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Extensions;
using ReactiveUI;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

/// <summary>
/// <see cref="PassFile"/> cell ViewModel.
/// </summary>
public class PassFileCellModel : ReactiveObject
{
    private readonly ObservableAsPropertyHelper<bool> _fullMode;

    public readonly PassFile PassFile;

    public PassFileCellModel(
        PassFile passFile,
        IObservable<bool> fullModeObservable,
        ReactCommand showCardCommand)
    {
        _fullMode = fullModeObservable.ToProperty(this, nameof(FullMode));

        PassFile = passFile;
        ShowCardCommand = showCardCommand;
        ShowCardCommand.Subscribe(RefreshState);

        fullModeObservable.Subscribe(_ => this.RaisePropertyChanged(nameof(Name)));
    }

    public bool FullMode => _fullMode.Value;

    public string Name => PassFile.IsLocalDeleted()
        ? '~' + (_fullMode.Value
            ? PassFile.Name
            : PassFile.Name[..1])
        : _fullMode.Value
            ? PassFile.Name
            : PassFile.Name[..2];

    public ISolidColorBrush? Color => PassFile.GetPassFileColor().Brush;

    public double Opacity => PassFile.IsLocalDeleted() ? 0.6d : 1d;

    public ISolidColorBrush StateColor => PassFile.GetStateColor();

    public ReactCommand ShowCardCommand { get; }

    private void RefreshState(Unit _)
    {
        this.RaisePropertyChanged(nameof(Name));
        this.RaisePropertyChanged(nameof(Color));
        this.RaisePropertyChanged(nameof(Opacity));
        this.RaisePropertyChanged(nameof(StateColor));
    }
}