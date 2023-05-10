using System;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Extensions;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Previews;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileWin;
using ReactiveUI;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

/// <summary>
/// <see cref="PassFile"/> cell ViewModel.
/// </summary>
public class PassFileCellModel<TPassFile> : ReactiveObject
    where TPassFile : PassFile
{
    private readonly Interaction<PassFileWinViewModel<TPassFile>, Unit>? _openInteraction;
    private readonly ObservableAsPropertyHelper<bool> _shortMode;

    /// <summary></summary>
    public readonly TPassFile PassFile;

    /// <summary></summary>
    public PassFileCellModel(
        TPassFile passFile,
        Interaction<PassFileWinViewModel<TPassFile>, Unit>? openInteraction,
        IObservable<bool> shortModeObservable)
    {
        PassFile = passFile;
        _openInteraction = openInteraction;
        _shortMode = shortModeObservable.ToProperty(this, nameof(FullMode));

        shortModeObservable.Subscribe(_ => this.RaisePropertyChanged(nameof(Name)));
    }

    #region preview

    /// <summary></summary>
    [Obsolete("PREVIEW constructor")]
    public PassFileCellModel() : this(
        PassFilePreviews.GetPassFile<TPassFile>(), null, Observable.Return(false))
    {
    }

    #endregion

    /// <summary></summary>
    public bool FullMode => !_shortMode.Value;
    
    /// <summary></summary>
    public string Name => PassFile.IsLocalDeleted()
        ? '~' + (_shortMode.Value
            ? PassFile.Name[..1]
            : PassFile.Name)
        : _shortMode.Value
            ? PassFile.Name[..2]
            : PassFile.Name;

    /// <summary></summary>
    public ISolidColorBrush? Color => PassFile.GetPassFileColor().Brush;

    /// <summary></summary>
    public double Opacity => PassFile.IsLocalDeleted() ? 0.6d : 1d;

    /// <summary></summary>
    public ISolidColorBrush StateColor => PassFile.GetStateColor();

    /// <summary></summary>
    public ReactCommand OpenCommand => ReactiveCommand.Create(OpenCard);

    /// <summary></summary>
    public void OpenCard() 
        => _openInteraction?
            .Handle(new PassFileWinViewModel<TPassFile>(PassFile))
            .Subscribe(_ => RefreshState());

    private void RefreshState()
    {
        this.RaisePropertyChanged(nameof(Name));
        this.RaisePropertyChanged(nameof(Color));
        this.RaisePropertyChanged(nameof(Opacity));
        this.RaisePropertyChanged(nameof(StateColor));
    }
}