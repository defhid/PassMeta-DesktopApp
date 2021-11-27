namespace PassMeta.DesktopApp.Ui.Models.Storage
{
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using DesktopApp.Common.Models.Entities;
    using Avalonia.Media;
    using Common.Interfaces.Services;
    using DynamicData.Binding;
    using PassFile;
    using ReactiveUI;
    using Splat;
    using Utils.Extensions;
    using Views.Main;
    using Views.Storage;

    public class PassFileBtn : ReactiveObject
    {
        private PassFile _passFile;
        public PassFile PassFile
        {
            get => _passFile;
            private set => this.RaiseAndSetIfChanged(ref _passFile, value);
        }

        private readonly ObservableAsPropertyHelper<PassFileColor> _color;
        public PassFileColor Color => _color.Value;
        
        private readonly ObservableAsPropertyHelper<ISolidColorBrush?> _stateColor;
        public ISolidColorBrush? StateColor => _stateColor.Value;
        
        private readonly ObservableAsPropertyHelper<double> _opacity;
        public double Opacity => _opacity.Value;
        
        private readonly ObservableAsPropertyHelper<string> _name;
        public string Name => _name.Value;

        private bool _shortMode;
        public bool ShortMode
        {
            get => _shortMode;
            set => this.RaiseAndSetIfChanged(ref _shortMode, value);
        }

        public PassFileBtn(PassFile passFile)
        {
            _passFile = passFile;

            _color = this.WhenValueChanged(btn => btn.PassFile)
                .Select(pf => pf!.GetPassFileColor())
                .ToProperty(this, btn => btn.Color);
            
            _stateColor = this.WhenValueChanged(btn => btn.PassFile)
                .Select(pf => pf!.GetStateColor())
                .ToProperty(this, btn => btn.StateColor);
            
            _opacity = this.WhenValueChanged(btn => btn.PassFile)
                .Select(pf => pf!.IsArchived ? 0.5d : 1d)
                .ToProperty(this, btn => btn.Opacity);
            
            _name = this.WhenAnyValue(btn => btn.PassFile, btn => btn.ShortMode)
                .Select(val => val.Item1.IsArchived
                    ? '~' + (val.Item2 ? PassFile.Name[..1] : PassFile.Name)
                    : val.Item2 ? PassFile.Name[..2] : PassFile.Name)
                .ToProperty(this, btn => btn.Name);
        }

        public Task OpenAsync()
        {
            var win = new PassFileWindow(PassFile);
            win.Closed += (_, _) =>
            {
                if (win.PassFile is null)
                {
                    Locator.Current.GetService<IDialogService>()!.ShowInfoAsync("handled: " + win.PassFile?.Name)
                        .GetAwaiter().GetResult();
                    
                    // TODO: fire event
                    return;
                }

                PassFile = win.PassFile;
            };
            
            return win.ShowDialog(MainWindow.Current);
        }
    }
}