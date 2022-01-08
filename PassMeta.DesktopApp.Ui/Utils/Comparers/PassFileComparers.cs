namespace PassMeta.DesktopApp.Ui.Utils.Comparers
{
    using System;
    using System.Collections.Generic;
    using Common.Models.Entities;

    public class PassFileComparer : IComparer<PassFile>
    {
        public int Compare(PassFile? x, PassFile? y) 
            => string.Compare(x?.Name, y?.Name, StringComparison.CurrentCultureIgnoreCase);
    }
}