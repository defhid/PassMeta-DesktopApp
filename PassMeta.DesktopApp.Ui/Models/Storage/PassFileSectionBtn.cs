using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Ui.Models.Storage
{
    public class PassFileSectionBtn
    {
        public readonly PassFile.Section Section;
        
        public int Index { get; }

        public string Name => Section.Name;

        public PassFileSectionBtn(PassFile.Section section, int index)
        {
            Section = section;
            Index = index;
        }
    }
}