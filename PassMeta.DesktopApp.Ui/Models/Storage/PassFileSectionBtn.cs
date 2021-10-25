using PassMeta.DesktopApp.Common.Models.Entities;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.Storage
{
    public class PassFileSectionBtn : ReactiveObject
    {
        public readonly PassFile.Section Section;
        
        public int Index { get; }

        private string? _name;
        public string? Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public PassFileSectionBtn(PassFile.Section section, int index)
        {
            Section = section;
            Index = index;
            Refresh();
        }

        public void Refresh()
        {
            Name = Section.Name;
        }
    }
}