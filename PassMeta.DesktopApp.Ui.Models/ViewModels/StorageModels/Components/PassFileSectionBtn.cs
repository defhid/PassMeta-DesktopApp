using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.StorageModels.Components;

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