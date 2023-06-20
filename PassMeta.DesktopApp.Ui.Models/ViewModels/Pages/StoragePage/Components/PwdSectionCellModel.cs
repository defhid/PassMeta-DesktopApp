using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

/// <summary>
/// <see cref="PwdSection"/> cell ViewModel.
/// </summary>
public class PwdSectionCellModel : ReactiveObject
{    
    public readonly PwdSection Section;

    public string Name { get; set; }

    public PwdSectionCellModel(PwdSection section)
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