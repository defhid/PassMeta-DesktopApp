using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Ui.Models.Storage
{
    public class PassFileSectionItemBtn
    {
        public int Index { get; }
        
        public string What { get; set; }
        
        public string Value { get; set; }

        public PassFileSectionItemBtn(PassFile.Section.Item item, int index)
        {
            Index = index;
            What = item.What;
            Value = item.Value;
        }
    }
}