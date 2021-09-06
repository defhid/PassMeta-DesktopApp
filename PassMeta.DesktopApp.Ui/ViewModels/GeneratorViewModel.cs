using System;
using PassMeta.DesktopApp.Ui.ViewModels.Base;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class GeneratorViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/generator";

        private int _length = 10;
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

        public GeneratorViewModel(IScreen hostScreen) : base(hostScreen)
        {
            this.WhenAnyValue(vm => vm.Result)
                .Subscribe(res => Generated = !string.IsNullOrEmpty(res));
        }
    }
}