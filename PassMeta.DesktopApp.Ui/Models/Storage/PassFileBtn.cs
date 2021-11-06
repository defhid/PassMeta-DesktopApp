namespace PassMeta.DesktopApp.Ui.Models.Storage
{
    using DesktopApp.Common.Models.Entities;
    using Avalonia.Media;
    using ReactiveUI;
    
    public class PassFileBtn : ReactiveObject
    {
        public readonly PassFile PassFile;

        private IBrush? _foreground;
        public IBrush? Foreground
        {
            get => _foreground;
            private set => this.RaiseAndSetIfChanged(ref _foreground, value);
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
            Foreground = PassFile.Color is null
                ? Brushes.AliceBlue
                : Brush.Parse("#" + PassFile.Color);

            Opacity = PassFile.IsArchived ? 0.5d : 1d;

            _SetName();
        }
    }
}