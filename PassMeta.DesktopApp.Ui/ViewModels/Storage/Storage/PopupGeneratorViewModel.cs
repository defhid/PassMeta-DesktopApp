using PassMeta.DesktopApp.Common.Abstractions.AppConfig;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Core.Extensions;
using Splat;

namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage;

using System;
using Utils;
using ReactiveUI;

using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

public class PopupGeneratorViewModel : ReactiveObject
{
    private readonly IPassMetaRandomService _pmRandomService = Locator.Current.Resolve<IPassMetaRandomService>();

    private int _length = Locator.Current.Resolve<IAppConfigProvider>().Current.DefaultPasswordLength;
    public int Length
    {
        get => _length;
        set => this.RaiseAndSetIfChanged(ref _length, value);
    }

    public bool IncludeDigits { get; set; } = PresetsCache.Generator.IncludeDigits;
    public bool IncludeLetters { get; set; } = PresetsCache.Generator.IncludeLowercase || PresetsCache.Generator.IncludeUppercase;
    public bool IncludeSpecial { get; set; } = PresetsCache.Generator.IncludeSpecial;
        
    public IObservable<bool> IsOpen { get; }

    public ReactCommand ResultApplyCommand { get; }

    public PopupGeneratorViewModel(IObservable<bool> isOpen, Action<string> apply)
    {
        IsOpen = isOpen;
            
        ResultApplyCommand = ReactiveCommand.Create(() => 
            apply(_pmRandomService.GeneratePassword(
                Length, IncludeDigits, IncludeLetters, IncludeLetters, IncludeSpecial)));
    }
}