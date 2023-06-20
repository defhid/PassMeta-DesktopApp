using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Extra;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

public class PassFileData : ReactiveObject
{

    private PwdPassFile? _passFile;
    public PwdPassFile? PassFile
    {
        get => _passFile;
        set
        {
            _passFile = value;
            _lastPassFileItemPath.PassFileId = value?.Id;
            _UpdatePassFileSectionList(true);
            if (_sectionsList.Any())
                _viewElements.SearchBox?.Focus();
        }
    }
        
    private readonly ObservableCollection<PwdSectionCellModel> _sectionsList = new();
    public ObservableCollection<PwdSectionCellModel>? SectionsList => _passFile is null ? null : _sectionsList;

    private readonly ObservableCollection<PwdItemReadModel> _sectionItemsList = new();
    public ObservableCollection<PwdItemReadModel>? SectionItemsList => _selectedSectionIndex < 0 ? null : _sectionItemsList;
        
    public IObservable<bool> IsSectionsBarVisible { get; }
    public IObservable<bool> IsItemsBarVisible { get; }
        
    public IObservable<string> NoSectionsText { get; }
    public bool IsNoSectionsTextVisible => !_sectionsList.Any();
    public bool IsNoItemsTextVisible => !_sectionItemsList.Any() && !Edit.Mode;

    private int _selectedSectionIndex = -1;
    public int SelectedSectionIndex
    {
        get => _selectedSectionIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSectionIndex, value);
            Edit.SectionBtn = value < 0 ? null : _sectionsList[value];
            _UpdatePassFileSectionItemList();
        }
    }
        
    public PwdSectionCellModel? SelectedSectionBtn =>
        _selectedSectionIndex < 0 ? null : _sectionsList[_selectedSectionIndex];

    public PwdSection? SelectedSection =>
        _selectedSectionIndex < 0 ? null : _sectionsList[_selectedSectionIndex].Section;
        
    private string? _searchText;
    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public PassFileDataEdit Edit { get; }

    private readonly IObservable<bool> _editModeObservable;

    private int _searching;

    private bool _addingSectionMode;

    private readonly PassFileBarExpander _passFileBarExpander;

    private readonly PassFileItemPath _lastPassFileItemPath;


    private readonly IPassFileContext<PwdPassFile> _pfContext =
        Locator.Current.Resolve<IPassFileContextProvider>().For<PwdPassFile>();

    public PassFileData(PassFileItemPath lastPassFileItemPath, PassFileBarExpander passFileBarExpander)
    {
        _lastPassFileItemPath = lastPassFileItemPath;
        _passFileBarExpander = passFileBarExpander;
            
        Edit = new PassFileDataEdit();
            
        this.WhenAnyValue(vm => vm.SectionsList)
            .Subscribe(_ => _UpdatePassFileSectionItemList());

        IsSectionsBarVisible = this.WhenAnyValue(vm => vm.SectionsList)
            .Select(sections => sections is not null);

        IsItemsBarVisible = this.WhenAnyValue(vm => vm.SectionItemsList)
            .Select(items => items is not null);
            
        _sectionsList.CollectionChanged += (_, _) => this.RaisePropertyChanged(nameof(IsNoSectionsTextVisible));
        _sectionItemsList.CollectionChanged += (_, _) => this.RaisePropertyChanged(nameof(IsNoItemsTextVisible));
            
        _editModeObservable = Edit.WhenAnyValue(vm => vm.Mode);

        _editModeObservable.Subscribe(_ => this.RaisePropertyChanged(nameof(IsNoItemsTextVisible)));

        this.WhenAnyValue(vm => vm.SearchText).Subscribe(_Search);

        NoSectionsText = this.WhenAnyValue(vm => vm.SearchText)
            .Select(text => string.IsNullOrEmpty(text)
                ? Resources.STORAGE__NO_SECTIONS
                : Resources.STORAGE__NO_SECTIONS_FOUND);
    }

    private void _Search(string? text)
    {
        var searching = ++_searching;
        if (text is null) return;
            
        _UpdatePassFileSectionList(false);
                
        if (!string.IsNullOrWhiteSpace(text))
        {
            throw new NotImplementedException("Search is not implemented");
            
            // text = text.Trim();
            // for (var i = _sectionsList.Count - 1; i >= 0; --i)
            // {
            //     var sectionBtn = _sectionsList[i];
            //     if (sectionBtn.Section.Search.Contains(text))
            //     {
            //         continue;
            //     }
            //     if (sectionBtn.Section.Items.Any(item => item.Search.Contains(text)))
            //     {
            //         continue;
            //     }
            //     if (searching == _searching)
            //     {
            //         _sectionsList.RemoveAt(i);
            //     }
            // }
        }
    }
}