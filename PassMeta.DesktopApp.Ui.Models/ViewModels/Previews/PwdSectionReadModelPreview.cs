using System.Collections.Generic;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Previews;

/// <inheritdoc />
public class PwdSectionReadModelPreview : PwdSectionReadModel
{
    public PwdSectionReadModelPreview() : base(new PwdSection
    {
        Name = "Test name",
        WebsiteUrl = "website@example.com",
        Items = new List<PwdItem>
        {
            new()
            {
                Usernames = new[] { "example_login1", "example_login2" },
                Password = "example_pwd",
                Remark = "example_remark"
            },
            new()
            {
                Usernames = new[] { "example_login3" },
                Password = "example_pwd",
                Remark = "example_remark"
            },
        },
    })
    {
    }
}