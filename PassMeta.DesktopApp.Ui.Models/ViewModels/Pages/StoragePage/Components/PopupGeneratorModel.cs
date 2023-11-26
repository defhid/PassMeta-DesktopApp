using System;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Presets;
using ReactiveUI;
using Splat;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

/// <summary>
/// Popup generator ViewModel.
/// </summary>
public class PopupGeneratorModel : ReactiveObject, IActivatableViewModel
{
    private readonly PasswordGeneratorPresets _presets = Locator.Current.Resolve<IAppPresetsProvider>().PasswordGeneratorPresets;
    private int _length;

    public PopupGeneratorModel(IObservable<bool> isOpen, Action<string> apply)
    {
        IsOpen = isOpen;
        
        ResultApplyCommand = ReactiveCommand.Create(() => apply(
            Locator.Current.Resolve<IPassMetaRandomService>().GeneratePassword(
                Length, IncludeDigits, IncludeLetters, IncludeLetters, IncludeSpecial)));
        
        Length = _presets.Length;
        IncludeDigits = _presets.IncludeDigits;
        IncludeLetters = _presets.IncludeLowercase || _presets.IncludeUppercase;
        IncludeSpecial = _presets.IncludeSpecial;
    }

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

    public ViewModelActivator Activator { get; } = new();
}