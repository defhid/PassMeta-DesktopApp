namespace PassMeta.DesktopApp.Ui.ViewModels
{
    using Core;
    using DesktopApp.Common;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Ui.ViewModels.Base;
    
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using ReactiveUI;
    using Utils;

    public class GeneratorViewModel : PageViewModel
    {
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();
        private readonly ICryptoService _cryptoService = EnvironmentContainer.Resolve<ICryptoService>();
        private readonly IClipboardService _clipboardService = EnvironmentContainer.Resolve<IClipboardService>();

        public int Length
        {
            get => PresetsCache.Generator.Length;
            set => this.RaiseAndSetIfChanged(ref PresetsCache.Generator.Length, value);
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

        public GeneratorViewModel(IScreen hostScreen) : base(hostScreen)
        {
            _result = _cryptoService.GeneratePassword(Length, IncludeDigits, IncludeLowercase, IncludeUppercase, IncludeSpecial);

            GenerateCommand = ReactiveCommand.Create(_Generate);
            CopyCommand = ReactiveCommand.Create(_CopyResultAsync);
            
            Generated = this.WhenAnyValue(vm => vm.Result)
                .Select(res => res != string.Empty);
        }

        public override Task RefreshAsync()
        {
            Result = string.Empty;
            return Task.CompletedTask;
        }
        
        private void _Generate()
        {
            Result = _cryptoService.GeneratePassword(Length, IncludeDigits, IncludeLowercase, IncludeUppercase, IncludeSpecial);
        }

        private async Task _CopyResultAsync()
        {
            if (await _clipboardService.TrySetTextAsync(Result))
            {
                _dialogService.ShowInfo(Resources.GENERATOR__RESULT_COPIED);
            }
        }
    }
}