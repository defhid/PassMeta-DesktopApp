namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage.Components
{
    using Common.Models.Entities;
    using ReactiveUI;

    public class PassFileSectionBtn : ReactiveObject
    {
        public readonly PassFile.Section Section;

        public string Name => Section.Name;

        public PassFileSectionBtn(PassFile.Section section)
        {
            Section = section;
        }

        public void Refresh()
        {
            this.RaisePropertyChanged(nameof(Name));
        }
    }
}