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

    public class GeneratorViewModel : PageViewModel
    {
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();
        private readonly ICryptoService _cryptoService = EnvironmentContainer.Resolve<ICryptoService>();
        private readonly IClipboardService _clipboardService = EnvironmentContainer.Resolve<IClipboardService>();
        
        private static int _length = 12;
        public int Length
        {
            get => _length;
            set => this.RaiseAndSetIfChanged(ref _length, value);
        }
        
        private static bool _includeDigits = true;
        public bool IncludeDigits
        {
            get => _includeDigits;
            set => this.RaiseAndSetIfChanged(ref _includeDigits, value);
        }
        
        private static bool _includeLowercase = true;
        public bool IncludeLowercase
        {
            get => _includeLowercase;
            set => this.RaiseAndSetIfChanged(ref _includeLowercase, value);
        }
        
        private static bool _includeUppercase = true;
        public bool IncludeUppercase
        {
            get => _includeUppercase;
            set => this.RaiseAndSetIfChanged(ref _includeUppercase, value);
        }
        
        private static bool _includeSpecial = true;
        public bool IncludeSpecial
        {
            get => _includeSpecial;
            set => this.RaiseAndSetIfChanged(ref _includeSpecial, value);
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