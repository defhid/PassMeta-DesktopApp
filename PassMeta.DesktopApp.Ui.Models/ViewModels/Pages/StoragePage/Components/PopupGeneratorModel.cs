using System;
using System.Reactive.Linq;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Ui.Models.Cache;
using ReactiveUI;
using Splat;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

/// <summary>
/// Popup generator ViewModel.
/// </summary>
public class PopupGeneratorModel : ReactiveObject
{
    private int _length;

    /// <summary></summary>
    public PopupGeneratorModel(IObservable<bool> isOpen, Action<string> apply)
    {
        IsOpen = isOpen;

        var presets = Locator.Current.Resolve<GeneratorPresetsCache>();

        IncludeDigits = presets.IncludeDigits;
        IncludeLetters = presets.IncludeLowercase || presets.IncludeUppercase;
        IncludeSpecial = presets.IncludeSpecial;

        Length = Locator.Current.Resolve<IAppConfigProvider>().Current.DefaultPasswordLength;

        ResultApplyCommand = ReactiveCommand.Create(() => apply(
            Locator.Current.Resolve<IPassMetaRandomService>().GeneratePassword(
                Length, IncludeDigits, IncludeLetters, IncludeLetters, IncludeSpecial)));
    }

    /// <summary></summary>
    [Obsolete("PREVIEW constructor")]
    public PopupGeneratorModel() : this(Observable.Return(true), _ => {})
    {
    }
    
    /// <summary></summary>
    public int Length
    {
        get => _length;
        set => this.RaiseAndSetIfChanged(ref _length, value);
    }

    /// <summary></summary>
    public bool IncludeDigits { get; set; }

    /// <summary></summary>
    public bool IncludeLetters { get; set; }

    /// <summary></summary>
    public bool IncludeSpecial { get; set; }

    /// <summary></summary>
    public IObservable<bool> IsOpen { get; }

    /// <summary></summary>
    public ReactCommand ResultApplyCommand { get; }
}