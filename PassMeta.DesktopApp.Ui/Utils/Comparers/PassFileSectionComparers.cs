namespace PassMeta.DesktopApp.Ui.Utils.Comparers
{
    using System;
    using System.Collections.Generic;
    using Common.Models.Entities;

    public class PassFileSectionComparer : IComparer<PassFile.Section>
    {
        public int Compare(PassFile.Section? x, PassFile.Section? y)
            => string.Compare(x?.Name, y?.Name, StringComparison.CurrentCultureIgnoreCase);
    }
}