using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Previews;

/// <inheritdoc />
public class PwdItemReadModelPreview : PwdItemReadModel
{
    public PwdItemReadModelPreview() : base(new PwdItem
    {
        Usernames = new[] { "example_login1", "example_login2" },
        Password = "example_pwd", 
        Remark = "example_remark"
    })
    {
    }
}