using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Presets;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages;

/// <summary>
/// Generator page ViewModel.
/// </summary>
public class GeneratorPageModel : PageViewModel
{
    private readonly IDialogService _dialogService = Locator.Current.Resolve<IDialogService>();
    private readonly IPassMetaRandomService _pmRandomService = Locator.Current.Resolve<IPassMetaRandomService>();
    private readonly IClipboardService _clipboardService = Locator.Current.Resolve<IClipboardService>();
    private readonly PasswordGeneratorPresets _presets = Locator.Current.Resolve<IAppPresetsProvider>().PasswordGeneratorPresets;

    private string _result = string.Empty;
    private int _length;
    private bool _includeDigits;
    private bool _includeLowercase;
    private bool _includeUppercase;
    private bool _includeSpecial;

    public GeneratorPageModel(IScreen hostScreen) : base(hostScreen)
    {
        GenerateCommand = ReactiveCommand.Create(Generate);
        CopyCommand = ReactiveCommand.Create(CopyResultAsync);

        Generated = this.WhenAnyValue(vm => vm.Result)
            .Select(res => res != string.Empty);
        
        Length = _presets.Length;
        IncludeDigits = _presets.IncludeDigits;
        IncludeLowercase = _presets.IncludeLowercase;
        IncludeUppercase = _presets.IncludeUppercase;
        IncludeSpecial = _presets.IncludeSpecial;

        this.WhenActivated(d => d(
            this.WhenAnyValue(
                x => x.Length,
                x => x.IncludeDigits,
                x => x.IncludeLowercase,
                x => x.IncludeUppercase,
                x => x.IncludeSpecial)
                .Subscribe(_ => Generate())));
    }

    public int Length
    {
        get => _length;
        set => this.RaiseAndSetIfChanged(ref _length, value);
    }

    public bool IncludeDigits
    {
        get => _includeDigits;
        set => this.RaiseAndSetIfChanged(ref _includeDigits, value);
    }

    public bool IncludeLowercase
    {
        get => _includeLowercase;
        set => this.RaiseAndSetIfChanged(ref _includeLowercase, value);
    }

    public bool IncludeUppercase
    {
        get => _includeUppercase;
        set => this.RaiseAndSetIfChanged(ref _includeUppercase, value);
    }

    public bool IncludeSpecial
    {
        get => _includeSpecial;
        set => this.RaiseAndSetIfChanged(ref _includeSpecial, value);
    }

    public string Result
    {
        get => _result;
        set => this.RaiseAndSetIfChanged(ref _result, value);
    }

    public IObservable<bool> Generated { get; }

    public ICommand GenerateCommand { get; }

    public ICommand CopyCommand { get; }

    /// <inheritdoc />
    public override ValueTask RefreshAsync()
    {
        Result = string.Empty;
        return ValueTask.CompletedTask;
    }

    private void Generate()
    {
        Result = _pmRandomService.GeneratePassword(
            Length, IncludeDigits, IncludeLowercase, IncludeUppercase, IncludeSpecial);
    }

    private async Task CopyResultAsync()
    {
        if (await _clipboardService.TrySetTextAsync(Result))
        {
            _dialogService.ShowInfo(Resources.GENERATOR__RESULT_COPIED);
        }
    }
}