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
    public PwdSectionReadModel(PwdSection section)
    {
        Name = section.Name;
        WebsiteUrl = section.WebsiteUrl;
        Items = section.Items.Select(x => new PwdItemReadModel(x)).ToList();
    }

    public string Name { get; }

    public string WebsiteUrl { get; }

    public List<PwdItemReadModel> Items { get; }

    public bool HasWebsiteUrl => !string.IsNullOrWhiteSpace(WebsiteUrl);

    public bool HasItems => Items.Count == 0;
}