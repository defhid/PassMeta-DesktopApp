namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage
{
    using System;
    using PassMeta.DesktopApp.Common.Interfaces.Services;
    using PassMeta.DesktopApp.Core;
    using PassMeta.DesktopApp.Ui.Utils;
    using ReactiveUI;

    using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

    public class PopupGeneratorViewModel : ReactiveObject
    {
        private readonly ICryptoService _cryptoService = EnvironmentContainer.Resolve<ICryptoService>();

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
                apply(_cryptoService.GeneratePassword(
                    Length, IncludeDigits, IncludeLetters, IncludeLetters, IncludeSpecial)));
        }
    }
}