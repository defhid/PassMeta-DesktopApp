namespace PassMeta.DesktopApp.Ui.Utils.Comparers
{
    using System;
    using System.Collections.Generic;
    using Common.Models.Entities;

    public class PassFileSectionComparer : IComparer<PassFile.PwdSection>
    {
        public int Compare(PassFile.PwdSection? x, PassFile.PwdSection? y)
            => string.Compare(x?.Name, y?.Name, StringComparison.CurrentCultureIgnoreCase);
    }
}