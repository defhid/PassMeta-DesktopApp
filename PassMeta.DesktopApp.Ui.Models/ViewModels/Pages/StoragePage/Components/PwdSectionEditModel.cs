using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using ReactiveUI;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

/// <summary>
/// <see cref="PwdSection"/> edit card ViewModel.
/// </summary>
public class PwdSectionEditModel : ReactiveObject
{
    private readonly Guid _id;

    /// <summary></summary>
    public readonly Interaction<Unit, Unit> ScrollToBottom = new();

    /// <summary></summary>
    private PwdSectionEditModel(Guid id)
    {
        _id = id;
        Items = new ObservableCollection<PwdItemEditModel>();
        Items.CollectionChanged += (_, ev) =>
        {
            if (ev.Action is not NotifyCollectionChangedAction.Add) return;
            foreach (PwdItemEditModel item in ev.NewItems!)
            {
                item.OnDelete += DeleteItem;
                item.OnMove += MoveItem;
            }
        };
        AddItemCommand = ReactiveCommand.Create(AddItem);
    }

    #region preview

    /// <summary></summary>
    [Obsolete("PREVIEW constructor")]
    public PwdSectionEditModel() : this(Guid.Empty)
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

    #endregion

    /// <summary></summary>
    public ReactCommand AddItemCommand { get; }

    /// <summary></summary>
    public string? Name { get; set; }

    /// <summary></summary>
    public string? WebsiteUrl { get; set; }

    /// <summary></summary>
    public ObservableCollection<PwdItemEditModel> Items { get; }

    /// <summary></summary>
    public bool HasItems => Items.Count == 0;
    
    /// <summary></summary>
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

    /// <summary></summary>
    public static PwdSectionEditModel From(PwdSection section)
    {
        var vm = new PwdSectionEditModel(section.Id)
        {
            Name = section.Name,
            WebsiteUrl = section.WebsiteUrl,
        };

        foreach (var item in section.Items)
        {
            vm.Items.Add(PwdItemEditModel.From(item));
        }

        return vm;
    }

    /// <summary></summary>
    public static PwdSectionEditModel Empty() => new(Guid.NewGuid())
    {
        Name = string.Empty,
        WebsiteUrl = string.Empty
    };

    private void AddItem()
    {
        Items.Add(PwdItemEditModel.Empty());
        ScrollToBottom.Handle(Unit.Default);
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
    }
}