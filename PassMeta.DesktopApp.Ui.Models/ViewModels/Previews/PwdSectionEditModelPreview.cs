using System;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Previews;

/// <inheritdoc />
public class PwdSectionEditModelPreview : PwdSectionEditModel
{
    public PwdSectionEditModelPreview() : base(Guid.Empty)
    {
        Name = "Preview Name";
        WebsiteUrl = "website.net";
        Items.Add(PwdItemEditModel.From(new PwdItem
        {
            Usernames = new[] { "first" },
            Password = "lalala",
            Remark = "something",
        }));
        Items.Add(PwdItemEditModel.From(new PwdItem
        {
            Usernames = new[] { "second", "third", "forth" },
            Password = "hahaha",
            Remark = "my note",
        }));
    }
}