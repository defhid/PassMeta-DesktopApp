namespace PassMeta.DesktopApp.Ui.ViewModels.Components.Storage
{
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Avalonia.Media;
    using Common.Interfaces.Services;
    using Common.Models.Entities;
    using Constants;
    using DynamicData.Binding;
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
        
        private readonly ObservableAsPropertyHelper<ISolidColorBrush> _stateColor;
        public ISolidColorBrush StateColor => _stateColor.Value;
        
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
                .ToProperty(this, nameof(Color));
            
            _stateColor = this.WhenValueChanged(btn => btn.PassFile)
                .Select(pf => pf!.GetStateColor())
                .ToProperty(this, nameof(StateColor));
            
            _opacity = this.WhenValueChanged(btn => btn.PassFile)
                .Select(pf => pf!.LocalDeleted ? 0.5d : 1d)
                .ToProperty(this, nameof(Opacity));
            
            _name = this.WhenAnyValue(btn => btn.PassFile, btn => btn.ShortMode)
                .Select(val => val.Item1.LocalDeleted
                    ? '~' + (val.Item2 ? PassFile.Name[..1] : PassFile.Name)
                    : val.Item2 ? PassFile.Name[..2] : PassFile.Name)
                .ToProperty(this, nameof(Name));
        }

        public Task OpenAsync()
        {
            var win = new PassFileWindow(PassFile);
            win.Closed += (_, _) =>
            {
                if (win.PassFile is null)
                {
                    Locator.Current.GetService<IDialogService>()!.ShowInfo("handled: " + win.PassFile?.Name);
                    
                    // TODO: fire event
                    return;
                }

                PassFile = win.PassFile;
            };
            
            return win.ShowDialog(MainWindow.Current);
        }
    }
}