using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;

namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage
{
    using System;
    using Common.Abstractions.Services;
    using Core;
    using Utils;
    using ReactiveUI;

    using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

    public class PopupGeneratorViewModel : ReactiveObject
    {
        private readonly IPassMetaCryptoService _passMetaCryptoService = Locator.Current.Resolve<IPassMetaCryptoService>();

        private int _length = PresetsCache.Generator.Length;
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
                apply(_passMetaCryptoService.GeneratePassword(
                    Length, IncludeDigits, IncludeLetters, IncludeLetters, IncludeSpecial)));
        }
    }
}