using System;
using System.Collections.Generic;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

namespace PassMeta.DesktopApp.Ui.Models.Comparers;

public class PwdSectionCellModelComparer : IComparer<PwdSectionCellModel>
{
    private PwdSectionCellModelComparer()
    {
    }
    
    public int Compare(PwdSectionCellModel? x, PwdSectionCellModel? y)
        => string.Compare(x?.Name, y?.Name, StringComparison.CurrentCultureIgnoreCase);

    public static readonly PwdSectionCellModelComparer Instance = new();
}