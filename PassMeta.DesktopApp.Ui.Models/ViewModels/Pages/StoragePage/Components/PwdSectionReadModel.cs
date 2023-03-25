using System;
using System.Collections.Generic;
using System.Linq;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

/// <summary>
/// <see cref="PwdSection"/> view card ViewModel.
/// </summary>
public class PwdSectionReadModel : ReactiveObject
{
    /// <summary></summary>
    public PwdSectionReadModel(PwdSection section)
    {
        Name = section.Name;
        WebsiteUrl = section.WebsiteUrl;
        Items = section.Items.Select(x => new PwdItemReadModel(x)).ToList();
    }

    #region preview

    /// <summary></summary>
    [Obsolete("PREVIEW constructor")]
    public PwdSectionReadModel() : this(new PwdSection
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

    #endregion

    /// <summary></summary>
    public string Name { get; }

    /// <summary></summary>
    public string WebsiteUrl { get; }

    /// <summary></summary>
    public List<PwdItemReadModel> Items { get; }

    /// <summary></summary>
    public bool HasWebsiteUrl => !string.IsNullOrWhiteSpace(WebsiteUrl);

    /// <summary></summary>
    public bool HasItems => Items.Count == 0;
}