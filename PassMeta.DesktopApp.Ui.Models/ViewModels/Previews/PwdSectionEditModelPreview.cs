using System.Collections.Generic;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Previews;

/// <inheritdoc />
public class PwdSectionEditModelPreview : PwdSectionEditModel
{
    public PwdSectionEditModelPreview()
    {
        Show(new PwdSection
        {
            Name = "Preview Name",
            WebsiteUrl = "website.net",
            Items = new List<PwdItem>
            {
                new()
                {
                    Usernames = new[] { "first" },
                    Password = "lalala",
                    Remark = "something",
                },
                new()
                {
                    Usernames = new[] { "second", "third", "forth" },
                    Password = "hahaha",
                    Remark = "my note",
                }
            },
        });
    }
}