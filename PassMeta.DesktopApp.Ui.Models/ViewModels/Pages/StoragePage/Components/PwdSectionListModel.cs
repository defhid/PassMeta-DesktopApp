using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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
            .ObserveOn(RxApp.TaskpoolScheduler)
            .Subscribe(Search);
    }

    public IObservable<bool> IsNoSectionsTextVisible { get; }

    public IObservable<string> NoSectionsText { get; }

    public bool IsVisible { get; private set; }

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
        var index = List.FindIndex(x => x.Section.Id == section.Id);
        if (index < 0)
        {
            return;
        }

        List = List.Take(index).Concat(List.Skip(index + 1)).ToList();
        SelectedIndex = index == List.Count ? List.Count - 1 : index;
    }
    
    public void RefreshOnly(PwdSection section)
    {
        var index = List.FindIndex(x => x.Section.Id == section.Id);
        if (index < 0)
        {
            return;
        }

        List = List.Take(index).Append(ToCell(section)).Concat(List.Skip(index + 1)).ToList();
        SelectedIndex = index;
    }

    public void Show(IEnumerable<PwdSection> sections)
    {
        var list = sections.Select(ToCell).ToList();
        list.Sort(PwdSectionCellModelComparer.Instance);
        List = list;
        IsVisible = true;
        this.RaisePropertyChanged(nameof(IsVisible));
    }

    public void Hide()
    {
        IsVisible = false;
        this.RaisePropertyChanged(nameof(IsVisible));
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
        
        // Dispatcher.UiThread...
    }

    private static PwdSectionCellModel ToCell(PwdSection pwdSection) => new(pwdSection);
}