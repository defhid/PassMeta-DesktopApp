namespace PassMeta.DesktopApp.Ui.ViewModels
{
    using Core;
    using DesktopApp.Common;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Ui.ViewModels.Base;
    
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using ReactiveUI;

    public class GeneratorViewModel : ViewModelPage
    {
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();
        private readonly ICryptoService _cryptoService = EnvironmentContainer.Resolve<ICryptoService>();
        private readonly IClipboardService _clipboardService = EnvironmentContainer.Resolve<IClipboardService>();
        
        private int _length = 12;
        public int Length
        {
            get => _length;
            set => this.RaiseAndSetIfChanged(ref _length, value);
        }
        
        private bool _includeDigits = true;
        public bool IncludeDigits
        {
            get => _includeDigits;
            set => this.RaiseAndSetIfChanged(ref _includeDigits, value);
        }
        
        private bool _includeSpecial = true;
        public bool IncludeSpecial
        {
            get => _includeSpecial;
            set => this.RaiseAndSetIfChanged(ref _includeSpecial, value);
        }

        private string? _result;
        public string? Result
        {
            get => _result;
            set => this.RaiseAndSetIfChanged(ref _result, value);
        }

        private bool _generated;
        public bool Generated
        {
            get => _generated;
            set => this.RaiseAndSetIfChanged(ref _generated, value);
        }
        
        public ICommand GenerateCommand { get; }
        
        public ICommand CopyCommand { get; }

        public GeneratorViewModel(IScreen hostScreen) : base(hostScreen)
        {
            GenerateCommand = ReactiveCommand.Create(_Generate);
            CopyCommand = ReactiveCommand.Create(_CopyResultAsync);
            
            this.WhenAnyValue(vm => vm.Result)
                .Subscribe(res => Generated = !string.IsNullOrEmpty(res));
        }

        public override Task RefreshAsync()
        {
            Result = null;
            return Task.CompletedTask;
        }
        
        private void _Generate()
        {
            Result = _cryptoService.GeneratePassword(Length, IncludeDigits, IncludeSpecial);
        }

        private async Task _CopyResultAsync()
        {
            if (await _clipboardService.TrySetTextAsync(Result ?? string.Empty))
            {
                _dialogService.ShowInfo(Resources.GENERATOR__RESULT_COPIED);
            }
        }
    }
}