using System.Reactive.Linq;
using Avalonia.Media;
using PassMeta.DesktopApp.Common.Models.Entities;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.Storage
{
    public class PassFileBtn : ReactiveObject
    {
        public readonly PassFile PassFile;

        public int Index { get; }
        
        public IBrush Foreground => PassFile.Color is null 
            ? Brushes.AliceBlue 
            : Brush.Parse( "#" + PassFile.Color);
        
        private bool _shortMode;
        public bool ShortMode
        {
            get => _shortMode;
            set => this.RaiseAndSetIfChanged(ref _shortMode, value);
        }

        private readonly ObservableAsPropertyHelper<string> _name;
        public string Name => _name.Value;
        
        public PassFileBtn(PassFile passFile, int index)
        {
            PassFile = passFile;
            Index = index;
            
            _name = this.WhenAnyValue(btn => btn.ShortMode)
                .Select(isShort => isShort ? PassFile.Name[..2] : PassFile.Name)
                .ToProperty(this, btn => btn.Name);
        }
    }
}