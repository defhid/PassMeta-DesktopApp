using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

namespace PassMeta.DesktopApp.Ui.Utils.Comparers
{
    using System;
    using System.Collections.Generic;

    public class PassFileSectionComparer : IComparer<PwdSection>
    {
        public int Compare(PwdSection? x, PwdSection? y)
            => string.Compare(x?.Name, y?.Name, StringComparison.CurrentCultureIgnoreCase);
    }
}