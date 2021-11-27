namespace PassMeta.DesktopApp.Ui.Models.Storage
{
    using DesktopApp.Common.Models.Entities;
    using Avalonia.Media;
    using Common.Interfaces.Services;
    using ReactiveUI;
    using Splat;
    using Utils.Extensions;
    using Views.Main;
    using Views.Storage;

    public class PassFileBtn : ReactiveObject
    {
        public readonly PassFile PassFile;

        private ISolidColorBrush? _color;
        public ISolidColorBrush? Color
        {
            get => _color;
            private set => this.RaiseAndSetIfChanged(ref _color, value);
        }

        private ISolidColorBrush? _stateColor;
        public ISolidColorBrush? StateColor
        {
            get => _stateColor;
            private set => this.RaiseAndSetIfChanged(ref _stateColor, value);
        }

        private double _opacity;
        public double Opacity
        {
            get => _opacity;
            private set => this.RaiseAndSetIfChanged(ref _opacity, value);
        }
        
        private bool _shortMode;
        public bool ShortMode
        {
            get => _shortMode;
            set
            {
                this.RaiseAndSetIfChanged(ref _shortMode, value);
                _SetName();
            }
        }

        private string? _name;
        public string? Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private PassFileWindow? _opened;
        
        public PassFileBtn(PassFile passFile)
        {
            PassFile = passFile;
            Refresh();
        }

        private void _SetName()
        {
            if (PassFile.IsArchived) 
                Name = '~' + (ShortMode ? PassFile.Name[..1] : PassFile.Name);
            else 
                Name = ShortMode ? PassFile.Name[..2] : PassFile.Name;
        }

        public void Refresh()
        {
            Color = PassFile.GetPassFileColor().Brush;
            StateColor = PassFile.GetStateColor();
            
            Opacity = PassFile.IsArchived ? 0.5d : 1d;
            _SetName();
        }
        
        #region Commands
        
        public void OpenCommand()
        {
            if (_opened is not null)
            {
                _opened.Activate();
                return;
            }

            _opened = new PassFileWindow(PassFile);
            _opened.Closed += (_, _) =>
            {
                var actual = _opened.PassFile;
                _opened = null;
                
                Locator.Current.GetService<IDialogService>()!.ShowInfoAsync("handled: " + actual?.Name)
                    .GetAwaiter().GetResult();
                // TODO
            };
            
            _opened.ShowDialog(MainWindow.Current);
        }
        
        #endregion
    }
}