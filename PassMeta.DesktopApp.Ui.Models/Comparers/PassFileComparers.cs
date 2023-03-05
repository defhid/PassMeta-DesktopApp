using System;
using System.Collections.Generic;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Ui.Models.Comparers;

public class PassFileComparer : IComparer<PassFile>
{
    public static readonly PassFileComparer Instance = new();
    
    public int Compare(PassFile? x, PassFile? y) 
        => string.Compare(x?.Name, y?.Name, StringComparison.CurrentCultureIgnoreCase);
}