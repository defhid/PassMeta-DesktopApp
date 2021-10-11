using System.Collections.Generic;
using Avalonia.Media;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Ui.Models.Storage
{
    public class PassFileBtn
    {
        private readonly PassFile _passFile;
        
        private readonly bool _shortMode;

        public int Id => _passFile.Id;

        public string Name => _shortMode ? _passFile.Name[..2] : _passFile.Name;

        public IBrush Foreground => _passFile.Color is null 
            ? Brushes.AliceBlue 
            : Brush.Parse( "#" + _passFile.Color);

        public List<PassFile.Section> Sections => _passFile.Data!;
        
        public bool Active { get; }

        public PassFileBtn(PassFile passFile, bool shortMode, bool active = false)
        {
            _passFile = passFile;
            _shortMode = shortMode;
            Active = active;
        }
    }
}