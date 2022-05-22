namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage.Components
{
    using Common.Models.Entities;
    using ReactiveUI;

    public class PassFileSectionBtn : ReactiveObject
    {
        public readonly PassFile.PwdSection Section;

        public string Name { get; set; }

        public PassFileSectionBtn(PassFile.PwdSection section)
        {
            Section = section;
            Name = Section.Name;
        }

        public void Refresh()
        {
            Name = Section.Name;
            this.RaisePropertyChanged(nameof(Name));
        }
    }
}