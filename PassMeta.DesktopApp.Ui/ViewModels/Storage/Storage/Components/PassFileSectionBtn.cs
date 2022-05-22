namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage.Components
{
    using Common.Models.Entities.Extra;
    using ReactiveUI;

    public class PassFileSectionBtn : ReactiveObject
    {
        public readonly PwdSection Section;

        public string Name { get; set; }

        public PassFileSectionBtn(PwdSection section)
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