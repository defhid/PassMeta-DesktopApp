namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage.Components
{
    using System;
    using Common.Interfaces.Services;
    using Core;
    using ReactiveUI;
    using Utils;
    
    using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

    public class PasswordGenerator : ReactiveObject
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

        public ReactCommand GenerateCommand { get; }

        public PasswordGenerator(Action<string> apply)
        {
            GenerateCommand = ReactiveCommand.Create(() => 
                apply(_cryptoService.GeneratePassword(
                    Length, IncludeDigits, IncludeLetters, IncludeLetters, IncludeSpecial)));
        }
    }
}