namespace PassMeta.DesktopApp.Ui.Models.Components.Storage
{
    using Common.Models.Entities;
    using ReactiveUI;

    public class PassFileSectionBtn : ReactiveObject
    {
        public readonly PassFile.Section Section;

        private string? _name;
        public string? Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public PassFileSectionBtn(PassFile.Section section)
        {
            Section = section;
            Refresh();
        }

        public void Refresh()
        {
            Name = Section.Name;
        }
    }
}