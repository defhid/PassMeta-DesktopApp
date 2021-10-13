using System.Collections.Generic;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Ui.Models.Storage
{
    public class PassFileSectionBtn
    {
        private readonly PassFile.Section _section;

        public string Name => _section.Name;

        public List<PassFile.Section.Item> Items => _section.Items;
        
        public int Index { get; }
        
        public bool Active { get; }

        public PassFileSectionBtn(PassFile.Section section, int index, bool active = false)
        {
            _section = section;
            Index = index;
            Active = active;
        }
    }
}