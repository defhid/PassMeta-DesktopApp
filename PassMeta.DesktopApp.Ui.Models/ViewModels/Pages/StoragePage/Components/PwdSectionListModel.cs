using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Ui.Models.Comparers;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

/// <summary>
/// <see cref="PwdSection"/> list ViewModel.
/// </summary>
public class PwdSectionListModel : ReactiveObject
{
    private IReadOnlyList<PwdSectionCellModel> _list = Array.Empty<PwdSectionCellModel>();
    private int _selectedIndex = -1;
    private bool _isReadOnly;
    private string? _searchText;
    private Thickness _margin;

    public PwdSectionListModel()
    {
        NoSectionsText = this.WhenAnyValue(vm => vm.SearchText)
            .Select(text => string.IsNullOrEmpty(text)
                ? Resources.STORAGE__NO_SECTIONS
                : Resources.STORAGE__NO_SECTIONS_FOUND);
        
        IsNoSectionsTextVisible = this.WhenAnyValue(x => x.List)
            .Select(x => x.Count == 0);

        this.WhenAnyValue(vm => vm.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(Search);
    }

    public IObservable<bool> IsNoSectionsTextVisible { get; }

    public IObservable<string> NoSectionsText { get; }

    public Thickness Margin
    {
        get => _margin;
        set => this.RaiseAndSetIfChanged(ref _margin, value);
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => this.RaiseAndSetIfChanged(ref _isReadOnly, value);
    }
    
    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public IReadOnlyList<PwdSectionCellModel> List
    {
        get => _list;
        private set => this.RaiseAndSetIfChanged(ref _list, value);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
    }

    public PwdSection? GetSelectedSection()
        => _selectedIndex < 0 ? null : _list[_selectedIndex].Section;

    public void Add()
    {
        var cell = ToCell(new PwdSection
        {
            Name = Resources.STORAGE__SECTION_NEW_NAME,
            Mark = PwdSectionMark.Created,
        });

        List = new[] { cell }.Concat(List).ToList();
        SelectedIndex = 0;
    }
    
    public void Remove(PwdSection section)
    {
        var index = List.FindIndex(x => x.Section.Id != section.Id);
        if (index < 0)
        {
            return;
        }

        List = List.Take(index).Concat(List.Skip(index + 1)).ToList();
        SelectedIndex = index == List.Count ? List.Count - 1 : index;
    }
    
    public void Refresh(PwdSection section)
    {
        var index = List.FindIndex(x => x.Section.Id != section.Id);
        if (index < 0)
        {
            return;
        }

        List = List.Take(index).Concat(new[] { ToCell(section) }).Concat(List.Skip(index + 1)).ToList();
        SelectedIndex = index;
    }

    public void RefreshList(IEnumerable<PwdSection> sections)
    {
        var list = sections.Select(ToCell).ToList();
        list.Sort(new PwdSectionCellModelComparer());
        List = list;
    }
    
    private void Search(string? text)
    {
        // var searching = ++_searching;
        // if (text is null) return;
        //     
        // _UpdatePassFileSectionList(false);
        //         
        // if (!string.IsNullOrWhiteSpace(text))
        // {
        //     throw new NotImplementedException("Search is not implemented");
        //     
        //     text = text.Trim();
        //     for (var i = _sectionsList.Count - 1; i >= 0; --i)
        //     {
        //         var sectionBtn = _sectionsList[i];
        //         if (sectionBtn.Section.Search.Contains(text))
        //         {
        //             continue;
        //         }
        //         if (sectionBtn.Section.Items.Any(item => item.Search.Contains(text)))
        //         {
        //             continue;
        //         }
        //         if (searching == _searching)
        //         {
        //             _sectionsList.RemoveAt(i);
        //         }
        //     }
        // }
    }

    private static PwdSectionCellModel ToCell(PwdSection pwdSection) => new(pwdSection);
}