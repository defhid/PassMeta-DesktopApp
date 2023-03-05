using System;
using System.Collections.Generic;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

namespace PassMeta.DesktopApp.Ui.Models.Common.Comparers;

public class PassFileSectionComparer : IComparer<PwdSection>
{
    public int Compare(PwdSection? x, PwdSection? y)
        => string.Compare(x?.Name, y?.Name, StringComparison.CurrentCultureIgnoreCase);
}