using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages;

public class GeneratorPageModel : PageViewModel
{
    private readonly IDialogService _dialogService = Locator.Current.Resolve<IDialogService>();
    private readonly IPassMetaRandomService _pmRandomService = Locator.Current.Resolve<IPassMetaRandomService>();
    private readonly IClipboardService _clipboardService = Locator.Current.Resolve<IClipboardService>();

    private int _length = Locator.Current.Resolve<IAppConfigProvider>().Current.DefaultPasswordLength;
    public int Length
    {
        get => _length;
        set => this.RaiseAndSetIfChanged(ref _length, value);
    }

    public bool IncludeDigits
    {
        get => PresetsCache.Generator.IncludeDigits;
        set => this.RaiseAndSetIfChanged(ref PresetsCache.Generator.IncludeDigits, value);
    }

    public bool IncludeLowercase
    {
        get =>  PresetsCache.Generator.IncludeLowercase;
        set => this.RaiseAndSetIfChanged(ref  PresetsCache.Generator.IncludeLowercase, value);
    }

    public bool IncludeUppercase
    {
        get => PresetsCache.Generator.IncludeUppercase;
        set => this.RaiseAndSetIfChanged(ref PresetsCache.Generator.IncludeUppercase, value);
    }

    public bool IncludeSpecial
    {
        get => PresetsCache.Generator.IncludeSpecial;
        set => this.RaiseAndSetIfChanged(ref PresetsCache.Generator.IncludeSpecial, value);
    }

    private static string _result = string.Empty;
    public string Result
    {
        get => _result;
        set => this.RaiseAndSetIfChanged(ref _result, value);
    }

    public IObservable<bool> Generated { get; }
        
    public ICommand GenerateCommand { get; }
        
    public ICommand CopyCommand { get; }

    public GeneratorPageModel(IScreen hostScreen) : base(hostScreen)
    {
        _result = _pmRandomService.GeneratePassword(Length, IncludeDigits, IncludeLowercase, IncludeUppercase, IncludeSpecial);

        GenerateCommand = ReactiveCommand.Create(_Generate);
        CopyCommand = ReactiveCommand.Create(_CopyResultAsync);
            
        Generated = this.WhenAnyValue(vm => vm.Result)
            .Select(res => res != string.Empty);
    }

    /// <inheritdoc />
    public override ValueTask RefreshAsync()
    {
        Result = string.Empty;
        return ValueTask.CompletedTask;
    }
        
    private void _Generate()
    {
        Result = _pmRandomService.GeneratePassword(Length, IncludeDigits, IncludeLowercase, IncludeUppercase, IncludeSpecial);
    }

    private async Task _CopyResultAsync()
    {
        if (await _clipboardService.TrySetTextAsync(Result))
        {
            _dialogService.ShowInfo(Resources.GENERATOR__RESULT_COPIED);
        }
    }
}