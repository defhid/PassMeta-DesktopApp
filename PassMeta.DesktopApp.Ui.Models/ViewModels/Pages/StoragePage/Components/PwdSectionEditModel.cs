using System;
using System.Collections.ObjectModel;
using System.Linq;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using ReactiveUI;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

/// <summary>
/// <see cref="PwdSection"/> edit card ViewModel.
/// </summary>
public class PwdSectionEditModel : ReactiveObject
{
    private Guid _id;

    public PwdSectionEditModel()
    {
        Items = new ObservableCollection<PwdItemEditModel>();
        AddItemCommand = ReactiveCommand.Create(() => AddItem(PwdItemEditModel.Empty()));
    }

    public bool IsVisible { get; private set; }

    public ReactCommand AddItemCommand { get; }

    public string? Name { get; set; }

    public string? WebsiteUrl { get; set; }

    public ObservableCollection<PwdItemEditModel> Items { get; }

    public bool HasNoItems => Items.Count == 0;

    public PwdSection ToSection() => new()
    {
        Id = _id,
        Name = Name?.Trim() ?? string.Empty,
        WebsiteUrl = WebsiteUrl?.Trim() ?? string.Empty,
        Items = Items
            .Where(x => !x.IsEmpty)
            .Select(x => x.ToItem())
            .ToList()
    };

    public void Show(PwdSection section)
    {
        _id = section.Id;
        Name = section.Name;
        WebsiteUrl = section.WebsiteUrl;

        foreach (var item in section.Items)
        {
            AddItem(PwdItemEditModel.From(item));
        }

        IsVisible = true;
        this.RaisePropertyChanged(nameof(Name));
        this.RaisePropertyChanged(nameof(WebsiteUrl));
        this.RaisePropertyChanged(nameof(HasNoItems));
        this.RaisePropertyChanged(nameof(IsVisible));
    }

    public void Hide()
    {
        IsVisible = false;
        Items.Clear();
        WebsiteUrl = string.Empty;
        Name = string.Empty;
        _id = default;
        this.RaisePropertyChanged(nameof(IsVisible));
    }

    #region Items

    private void AddItem(PwdItemEditModel item)
    {
        item.OnDelete += DeleteItem;
        item.OnMove += MoveItem;
        Items.Add(item);
        this.RaisePropertyChanged(nameof(HasNoItems));
    }

    private void MoveItem(PwdItemEditModel item, int direction)
    {
        var index = Items.IndexOf(item);

        if (direction > 0 && index + direction < Items.Count ||
            direction < 0 && index + direction > -1)
        {
            Items.Move(index, index + direction);
        }
    }

    private void DeleteItem(PwdItemEditModel item)
    {
        Items.Remove(item);
        this.RaisePropertyChanged(nameof(HasNoItems));
    }

    #endregion
}