using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Ui.Models.Storage
{
    public class PassFileSectionItemBtn
    {
        public string What { get; set; }
        
        public string Value { get; set; }

        public int Index { get; }
        
        public bool Active { get; }

        public PassFileSectionItemBtn(PassFile.Section.Item item, int index, bool active = false)
        {
            What = item.What;
            Value = item.Value;
            Index = index;
            Active = active;
        }
    }
}