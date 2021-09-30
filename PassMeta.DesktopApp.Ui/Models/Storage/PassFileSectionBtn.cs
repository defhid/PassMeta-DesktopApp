using System.Collections.Generic;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Ui.Models.Storage
{
    public class PassFileSectionBtn
    {
        private readonly PassFile.Section _section;
        
        private readonly bool _shortMode;

        public string Name => _shortMode ? _section.Name[..2] : _section.Name;

        public List<PassFile.Section.Item> Items => _section.Items;
        
        public int Index { get; }
        
        public bool Active { get; }

        public PassFileSectionBtn(PassFile.Section section, int index, bool shortMode = false, bool active = false)
        {
            _section = section;
            _shortMode = shortMode;
            Index = index;
            Active = active;
        }
    }
}