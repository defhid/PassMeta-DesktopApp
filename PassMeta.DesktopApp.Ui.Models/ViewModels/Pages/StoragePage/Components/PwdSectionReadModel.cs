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
    public bool IsVisible { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string WebsiteUrl { get; private set; } = string.Empty;

    public IReadOnlyList<PwdItemReadModel> Items { get; private set; } = Array.Empty<PwdItemReadModel>();

    public bool HasWebsiteUrl => !string.IsNullOrWhiteSpace(WebsiteUrl);

    public bool HasNoItems => Items.Count == 0;
    
    public void Show(PwdSection section)
    {
        Name = section.Name;
        WebsiteUrl = section.WebsiteUrl;
        Items = section.Items.Select(x => new PwdItemReadModel(x)).ToList();
        IsVisible = true;

        this.RaisePropertyChanged(nameof(Name));
        this.RaisePropertyChanged(nameof(WebsiteUrl));
        this.RaisePropertyChanged(nameof(Items));
        this.RaisePropertyChanged(nameof(HasNoItems));
        this.RaisePropertyChanged(nameof(IsVisible));
    }

    public void Hide()
    {
        IsVisible = false;
        Items = Array.Empty<PwdItemReadModel>();
        WebsiteUrl = string.Empty;
        Name = string.Empty;
        this.RaisePropertyChanged(nameof(IsVisible));
    }
}