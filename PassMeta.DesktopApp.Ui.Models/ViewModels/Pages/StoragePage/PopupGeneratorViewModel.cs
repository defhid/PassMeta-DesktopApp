using System;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Ui.Models.Cache;
using ReactiveUI;
using Splat;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage;

public class PopupGeneratorViewModel : ReactiveObject
{
    private readonly IPassMetaRandomService _pmRandomService = Locator.Current.Resolve<IPassMetaRandomService>();
    private readonly GeneratorPresetsCache _presetsCache = Locator.Current.Resolve<GeneratorPresetsCache>();

    private int _length = Locator.Current.Resolve<IAppConfigProvider>().Current.DefaultPasswordLength;
    public int Length
    {
        get => _length;
        set => this.RaiseAndSetIfChanged(ref _length, value);
    }

    public bool IncludeDigits { get; set; }
    public bool IncludeLetters { get; set; }
    public bool IncludeSpecial { get; set; }
        
    public IObservable<bool> IsOpen { get; }

    public ReactCommand ResultApplyCommand { get; }

    public PopupGeneratorViewModel(IObservable<bool> isOpen, Action<string> apply)
    {
        IsOpen = isOpen;

        IncludeDigits = _presetsCache.IncludeDigits;
        IncludeLetters = _presetsCache.IncludeLowercase || _presetsCache.IncludeUppercase;
        IncludeSpecial = _presetsCache.IncludeSpecial;
            
        ResultApplyCommand = ReactiveCommand.Create(() => 
            apply(_pmRandomService.GeneratePassword(
                Length, IncludeDigits, IncludeLetters, IncludeLetters, IncludeSpecial)));
    }
}