using System;
using System.Threading.Tasks;
using Avalonia.Media;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Services;
using PassMeta.DesktopApp.Ui.Models.Extensions;
using PassMeta.DesktopApp.Ui.Models.Providers;
using ReactiveUI;
using Splat;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

/// <summary>
/// <see cref="PassFile"/> cell ViewModel.
/// </summary>
public class PassFileCellModel<TPassFile> : ReactiveObject
    where TPassFile : PassFile
{
    private readonly IPassFileOpenUiService<TPassFile> _openUiService = 
        Locator.Current.Resolve<IPassFileOpenUiService<TPassFile>>();
    
    private readonly HostWindowProvider _windowProvider;

    private readonly ObservableAsPropertyHelper<bool> _fullMode;

    /// <summary></summary>
    public readonly TPassFile PassFile;

    /// <summary></summary>
    public PassFileCellModel(
        TPassFile passFile,
        IObservable<bool> fullModeObservable,
        HostWindowProvider windowProvider)
    {
        _fullMode = fullModeObservable.ToProperty(this, nameof(FullMode));
        _windowProvider = windowProvider;

        PassFile = passFile;
        ShowCardCommand = ReactiveCommand.CreateFromTask(ShowCardAsync);

        fullModeObservable.Subscribe(_ => this.RaisePropertyChanged(nameof(Name)));
    }

    /// <summary></summary>
    public bool FullMode => _fullMode.Value;

    /// <summary></summary>
    public string Name => PassFile.IsLocalDeleted()
        ? '~' + (_fullMode.Value
            ? PassFile.Name
            : PassFile.Name[..1])
        : _fullMode.Value
            ? PassFile.Name
            : PassFile.Name[..2];

    /// <summary></summary>
    public ISolidColorBrush? Color => PassFile.GetPassFileColor().Brush;

    /// <summary></summary>
    public double Opacity => PassFile.IsLocalDeleted() ? 0.6d : 1d;

    /// <summary></summary>
    public ISolidColorBrush StateColor => PassFile.GetStateColor();

    /// <summary></summary>
    public ReactCommand ShowCardCommand { get; }

    /// <summary></summary>
    public async Task ShowCardAsync()
    {
        await _openUiService.ShowInfoAsync(PassFile, _windowProvider);
        RefreshState();
    }

    private void RefreshState()
    {
        this.RaisePropertyChanged(nameof(Name));
        this.RaisePropertyChanged(nameof(Color));
        this.RaisePropertyChanged(nameof(Opacity));
        this.RaisePropertyChanged(nameof(StateColor));
    }
}