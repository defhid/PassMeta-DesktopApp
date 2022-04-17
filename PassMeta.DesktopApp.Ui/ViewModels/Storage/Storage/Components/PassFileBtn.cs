namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage.Components
{
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Avalonia.Media;
    using Common.Models.Entities;
    using ReactiveUI;
    using Ui.Utils.Extensions;
    using Views.Main;
    using Views.Storage;
    
    using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

    public class PassFileBtn : ReactiveObject
    {
        private PassFile? _passFile;
        public PassFile? PassFile
        {
            get => _passFile;
            private set => this.RaiseAndSetIfChanged(ref _passFile, value);
        }

        public ISolidColorBrush StateColor { get; private set; }

        public IObservable<string> Name { get; }
         
        public IObservable<ISolidColorBrush?> Color { get; }

        public IObservable<double> Opacity { get; }

        public ReactCommand OpenCommand { get; }

        private readonly ObservableAsPropertyHelper<bool> _shortMode;
        private bool ShortMode => _shortMode.Value;
        
        public event EventHandler<PassFileChangedEventArgs>? PassFileChanged;

        public PassFileBtn(PassFile passFile, IObservable<bool> shortModeObservable)
        {
            _passFile = passFile;
            _shortMode = shortModeObservable.ToProperty(this, nameof(ShortMode));
            
            StateColor = passFile.GetStateColor();
            
            Name = this.WhenAnyValue(btn => btn.PassFile, btn => btn.ShortMode)
                .Select(pair => pair.Item1 is null 
                    ? "~"
                    : pair.Item1.LocalDeleted
                        ? '~' + (pair.Item2 ? pair.Item1.Name[..1] : pair.Item1.Name)
                        : pair.Item2 ? pair.Item1.Name[..2] : pair.Item1.Name);
            
            var passFileObservable = this.WhenAnyValue(btn => btn.PassFile);

            Color = passFileObservable.Select(pf => pf?.GetPassFileColor().Brush);

            Opacity = passFileObservable.Select(pf => pf?.LocalDeleted ?? true ? 0.6d : 1d);

            OpenCommand = ReactiveCommand.CreateFromTask(OpenAsync, passFileObservable.Select(pf => pf is not null));

        }

        public async Task OpenAsync()
        {
            var win = new PassFileWin(PassFile!);

            await win.ShowDialog(MainWindow.Current);
            
            if (win.PassFileChanged)
            {
                PassFileChanged?.Invoke(this, new PassFileChangedEventArgs(PassFile!, win.PassFile));
                PassFile = win.PassFile;
                RefreshState();
            }
        }

        public void RefreshState()
        {
            StateColor = PassFile.GetStateColor();
            this.RaisePropertyChanged(nameof(StateColor));
        }
        
        public class PassFileChangedEventArgs : EventArgs
        {
            public readonly PassFile PassFileOld;
            public readonly PassFile? PassFileNew;

            public PassFileChangedEventArgs(PassFile pfOld, PassFile? pfNew)
            {
                PassFileOld = pfOld;
                PassFileNew = pfNew;
            }
        }
    }
}