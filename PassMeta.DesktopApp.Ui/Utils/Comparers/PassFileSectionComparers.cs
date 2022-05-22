namespace PassMeta.DesktopApp.Ui.Utils.Comparers
{
    using System;
    using System.Collections.Generic;
    using Common.Models.Entities.Extra;

    public class PassFileSectionComparer : IComparer<PwdSection>
    {
        public int Compare(PwdSection? x, PwdSection? y)
            => string.Compare(x?.Name, y?.Name, StringComparison.CurrentCultureIgnoreCase);
    }
}