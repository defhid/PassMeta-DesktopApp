using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Threading;
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
    private IReadOnlyList<PwdSectionCellModel> _fullList = Array.Empty<PwdSectionCellModel>();
    private IReadOnlyList<PwdSectionCellModel> _resultList = Array.Empty<PwdSectionCellModel>();
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

    public IReadOnlyList<PwdSectionCellModel> FullList
    {
        get => _fullList;
        set
        {
            _fullList = value;
            Search(SearchText);
        }
    }

    public IReadOnlyList<PwdSectionCellModel> List
    {
        get => _resultList;
        private set => this.RaiseAndSetIfChanged(ref _resultList, value);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
    }

    public PwdSection? GetSelectedSection()
        => _selectedIndex < 0 ? null : _fullList[_selectedIndex].Section;

    public void Add()
    {
        var cell = ToCell(new PwdSection
        {
            Name = Resources.STORAGE__SECTION_NEW_NAME,
            Mark = PwdSectionMark.Created,
        });

        FullList = FullList.Append(cell).ToList();
        SelectedIndex = List.FindIndex(x => x.Section.Id == cell.Section.Id);
    }

    public void Remove(PwdSection section)
    {
        var currIndex = SelectedIndex;
        var fullIndex = FullList.FindIndex(x => x.Section.Id == section.Id);
        if (fullIndex < 0)
        {
            return;
        }

        FullList = FullList.Take(fullIndex).Concat(FullList.Skip(fullIndex + 1)).ToList();
        SelectedIndex = currIndex == List.Count ? List.Count - 1 : currIndex;
    }
    
    public void RefreshOnly(PwdSection section)
    {
        var fullIndex = FullList.FindIndex(x => x.Section.Id == section.Id);
        if (fullIndex < 0)
        {
            return;
        }

        FullList = FullList.Take(fullIndex).Append(ToCell(section)).Concat(FullList.Skip(fullIndex + 1)).ToList();
        SelectedIndex = List.FindIndex(x => x.Section.Id == section.Id);
    }

    public void Show(IEnumerable<PwdSection> sections)
    {
        var list = sections.Select(ToCell).ToList();
        list.Sort(PwdSectionCellModelComparer.Instance);

        FullList = list;
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
        if (string.IsNullOrWhiteSpace(text))
        {
            Dispatcher.UIThread.Invoke(() => List = FullList);
            return;
        }

        var result = new List<PwdSectionCellModel>(FullList.Count / 2);

        text = text.Trim().ToLower();

        for (var i = FullList.Count - 1; i >= 0; --i)
        {
            var section = FullList[i];

            if (section.Section.Name.ToLower().Contains(text) ||
                section.Section.WebsiteUrl.ToLower().Contains(text))
            {
                result.Add(section);
            }
        }

        Dispatcher.UIThread.Invoke(() => List = result);
    }

    private static PwdSectionCellModel ToCell(PwdSection pwdSection) => new(pwdSection);
}