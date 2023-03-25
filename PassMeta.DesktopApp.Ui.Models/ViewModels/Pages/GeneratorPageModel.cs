using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Ui.Models.Cache;
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
    private readonly IAppConfigProvider _appConfig = Locator.Current.Resolve<IAppConfigProvider>();
    private readonly GeneratorPresetsCache _presetsCache = Locator.Current.Resolve<GeneratorPresetsCache>();

    private string _result = string.Empty;
    private int _length;

    /// <summary></summary>
    public GeneratorPageModel(IScreen hostScreen) : base(hostScreen)
    {
        _length = _appConfig.Current.DefaultPasswordLength;
        Generate();

        GenerateCommand = ReactiveCommand.Create(Generate);
        CopyCommand = ReactiveCommand.Create(CopyResultAsync);

        Generated = this.WhenAnyValue(vm => vm.Result)
            .Select(res => res != string.Empty);
    }

    #region preview

    /// <summary></summary>
    [Obsolete("PREVIEW constructor")]
    public GeneratorPageModel() : this(null!)
    {
    }

    #endregion

    /// <summary></summary>
    public int Length
    {
        get => _length;
        set => this.RaiseAndSetIfChanged(ref _length, value);
    }

    /// <summary></summary>
    public bool IncludeDigits
    {
        get => _presetsCache.IncludeDigits;
        set => _presetsCache.IncludeDigits = value;
    }

    /// <summary></summary>
    public bool IncludeLowercase
    {
        get => _presetsCache.IncludeLowercase;
        set => _presetsCache.IncludeLowercase = value;
    }

    /// <summary></summary>
    public bool IncludeUppercase
    {
        get => _presetsCache.IncludeUppercase;
        set => _presetsCache.IncludeUppercase = value;
    }

    /// <summary></summary>
    public bool IncludeSpecial
    {
        get => _presetsCache.IncludeSpecial;
        set => _presetsCache.IncludeSpecial = value;
    }

    /// <summary></summary>
    public string Result
    {
        get => _result;
        set => this.RaiseAndSetIfChanged(ref _result, value);
    }

    /// <summary></summary>
    public IObservable<bool> Generated { get; }

    /// <summary></summary>
    public ICommand GenerateCommand { get; }

    /// <summary></summary>
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