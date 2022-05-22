namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage.Components
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Avalonia;
    using Avalonia.Controls;
    using Common;
    using Common.Interfaces.Services;
    using Common.Models.Entities;
    using Common.Utils.Extensions;
    using Core;
    using Core.Utils;
    using Core.Utils.Extensions;
    using Models;
    using ReactiveUI;
    using Utils.Comparers;
    using Views.Main;

    public class PassFileData : ReactiveObject
    {
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();
        
        public IEnumerable<ContentControl> RightBarButtons => new ContentControl[]
        {
            new Button
            {
                Content = "\uE70F",
                
                Command = ReactiveCommand.Create(ItemsEdit),
                [!Button.IsVisibleProperty] = this.WhenAnyValue(vm => vm.SectionItemsList, vm => vm.Edit.Mode)
                    .Select(x => x.Item1 is not null && !x.Item2)
                    .ToBinding(),
                [ToolTip.TipProperty] = Resources.STORAGE__RIGHT_BAR_TOOLTIP__EDIT_ITEMS,
                [ToolTip.PlacementProperty] = PlacementMode.Left
            },
            new Button
            {
                Content = "\uE711", 
                Command = ReactiveCommand.Create(ItemsDiscardChanges),
                [!Button.IsVisibleProperty] = this.WhenAnyValue(vm => vm.SectionItemsList, vm => vm.Edit.Mode)
                    .Select(x => x.Item1 is not null && x.Item2)
                    .ToBinding(),
                [ToolTip.TipProperty] = Resources.STORAGE__RIGHT_BAR_TOOLTIP__DISCARD_ITEMS,
                [ToolTip.PlacementProperty] = PlacementMode.Left
            },
            new Button
            {
                Content = "\uE8FB", 
                Command = ReactiveCommand.Create(ItemsApplyChanges),
                [!Button.IsVisibleProperty] = this.WhenAnyValue(vm => vm.SectionItemsList, vm => vm.Edit.Mode)
                    .Select(x => x.Item1 is not null && x.Item2)
                    .ToBinding(),
                [ToolTip.TipProperty] = Resources.STORAGE__RIGHT_BAR_TOOLTIP__APPLY_ITEMS,
                [ToolTip.PlacementProperty] = PlacementMode.Left
            },
            new Button
            {
                Content = "\uE74D", 
                Command = ReactiveCommand.Create(SectionDeleteAsync),
                [!Button.IsVisibleProperty] = this.WhenAnyValue(vm => vm.SectionItemsList, vm => vm.Edit.Mode)
                    .Select(x => x.Item1 is not null && !x.Item2)
                    .ToBinding(),
                [ToolTip.TipProperty] = Resources.STORAGE__RIGHT_BAR_TOOLTIP__DELETE_SECTION,
                [ToolTip.PlacementProperty] = PlacementMode.Left
            },
        };
        
        private PassFile? _passFile;
        public PassFile? PassFile
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
        
        private readonly ObservableCollection<PassFileSectionBtn> _sectionsList = new();
        public ObservableCollection<PassFileSectionBtn>? SectionsList => _passFile is null ? null : _sectionsList;

        private readonly ObservableCollection<PassFileSectionItemBtn> _sectionItemsList = new();
        public ObservableCollection<PassFileSectionItemBtn>? SectionItemsList => _selectedSectionIndex < 0 ? null : _sectionItemsList;
        
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
        
        public PassFileSectionBtn? SelectedSectionBtn =>
            _selectedSectionIndex < 0 ? null : _sectionsList[_selectedSectionIndex];

        public PassFile.PwdSection? SelectedSection =>
            _selectedSectionIndex < 0 ? null : _sectionsList[_selectedSectionIndex].Section;
        
        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        public PassFileDataEdit Edit { get; }

        public event EventHandler<EventArgs>? PassFileCanBeChanged;

        private readonly IObservable<bool> _editModeObservable;

        private int _searching;

        private bool _addingSectionMode;

        private readonly PassFileBarExpander _passFileBarExpander;

        private readonly PassFileItemPath _lastPassFileItemPath;

        private readonly ViewElements _viewElements;

        public PassFileData(ViewElements viewElements, PassFileItemPath lastPassFileItemPath, PassFileBarExpander passFileBarExpander)
        {
            _viewElements = viewElements;
            _lastPassFileItemPath = lastPassFileItemPath;
            _passFileBarExpander = passFileBarExpander;
            
            Edit = new PassFileDataEdit(viewElements);
            
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
                text = text.Trim();
                for (var i = _sectionsList.Count - 1; i >= 0; --i)
                {
                    var sectionBtn = _sectionsList[i];
                    if (sectionBtn.Section.Search.Contains(text))
                    {
                        continue;
                    }
                    if (sectionBtn.Section.Items.Any(item => item.Search.Contains(text)))
                    {
                        continue;
                    }
                    if (searching == _searching)
                    {
                        _sectionsList.RemoveAt(i);
                    }
                }
            }
        }

        #region Buttons factory

        private PassFileSectionBtn _MakePassFileSectionBtn(PassFile.PwdSection section) 
            => new(section);
        
        private PassFileSectionItemBtn _MakePassFileSectionItemBtn(PassFile.PwdSection.PwdItem item) 
            => new(item, _editModeObservable, ItemDelete, ItemMove);

        #endregion

        #region List updates

        private void _UpdatePassFileSectionList(bool clearSearch)
        {
            _sectionsList.Clear();
            if (clearSearch)
            {
                SearchText = null;
            }
            
            var list = PassFile?.DataPwd;
            if (list is not null)
            {
                list.Sort(new PassFileSectionComparer());
                foreach (var section in list)
                {
                    _sectionsList.Add(_MakePassFileSectionBtn(section));
                }
            }

            this.RaisePropertyChanged(nameof(SectionsList));
        }
        
        private void _UpdatePassFileSectionItemList()
        {
            _sectionItemsList.Clear();

            var section = SelectedSection;
            var items = section?.Items;
            if (items is not null)
            {
                foreach (var item in items)
                {
                    _sectionItemsList.Add(_MakePassFileSectionItemBtn(item));
                }
            }

            _lastPassFileItemPath.PassFileSectionId = section?.Id;

            this.RaisePropertyChanged(nameof(SectionItemsList));
        }

        #endregion
        
        #region Sections

        public void SectionAdd()
        {
            var section = new PassFile.PwdSection { Name = Resources.STORAGE__SECTION_NEW_NAME };

            using (_passFileBarExpander.DisableAutoExpandingScoped())
            {
                _sectionsList.Insert(0, _MakePassFileSectionBtn(section));
                SelectedSectionIndex = 0;
                _viewElements.SectionListBox!.ScrollIntoView(0);
            }

            ItemsEdit();
            ItemAdd();
            
            _addingSectionMode = true;
        }

        public async Task SectionDeleteAsync()
        {
            var passFile = _passFile!;
            var section = SelectedSection!;
            
            var confirm = await _dialogService.ConfirmAsync(string.Format(Resources.STORAGE__CONFIRM_DELETE_SECTION, section.Name));
            if (confirm.Bad) return;

            using var preloader = MainWindow.Current!.StartPreloader();

            var result = PassFileManager.UpdateDataSelectively(passFile, data => 
                data.RemoveAll(s => s.Id == section.Id));

            if (result.Ok)
            {
                var index = SelectedSectionIndex;
                
                using (_passFileBarExpander.DisableAutoExpandingScoped())
                {
                    _sectionsList.RemoveAt(index);
                    SelectedSectionIndex = Math.Min(index, _sectionsList.Count - 1);
                }
            }
            else
            {
                _dialogService.ShowError(result.Message!);
            }

            PassFileCanBeChanged?.Invoke(this, EventArgs.Empty);
        }
        
        #endregion

        #region Items

        public void ItemsEdit()
        {
            Edit.Mode = true;
        }

        private void ItemsApplyChanges()
        {
            using var preloader = MainWindow.Current!.StartPreloader();

            var passFile = _passFile!;
            var section = SelectedSection!;

            if (_addingSectionMode)
            {
                var res = PassFileManager.UpdateDataSelectively(passFile, data => 
                    data.Add(section.Copy()));

                if (res.Bad)
                {
                    _dialogService.ShowError(res.Message!);
                    return;
                }

                _addingSectionMode = false;
            }

            PassFileCanBeChanged?.Invoke(this, EventArgs.Empty);

            var items = _sectionItemsList.Select(btn => btn.ToItem()).ToList();
            var sectionName = Edit.SectionName?.Trim();

            if (string.IsNullOrEmpty(sectionName))
            {
                sectionName = section.Name;
            }

            if (!section.DiffersFrom(new PassFile.PwdSection { Name = sectionName, Items = items }))
            {
                Edit.Mode = false;
                return;
            }
            
            var result = PassFileManager.UpdateDataSelectively(passFile, data =>
            {
                var lSection = data.First(s => s.Id == section.Id);
                lSection.Name = sectionName;
                lSection.Items = items.Select(i => i.Copy()).ToList();
            });

            if (result.Ok)
            {
                using (_passFileBarExpander.DisableAutoExpandingScoped())
                {
                    _UpdatePassFileSectionList(true);
                    SelectedSectionIndex = _sectionsList.FindIndex(btn => btn.Section.Id == section.Id);
                    _UpdatePassFileSectionItemList();
                }
                
                Edit.Mode = false;
            }
            else
            {
                _dialogService.ShowError(result.Message!);
            }
        }

        private void ItemsDiscardChanges()
        {
            using (_passFileBarExpander.DisableAutoExpandingScoped())
            {
                if (_addingSectionMode)
                {
                    _sectionsList.RemoveAt(SelectedSectionIndex);
                    _addingSectionMode = false;
                }
                else
                {
                    SelectedSectionBtn!.Refresh();
                    _UpdatePassFileSectionItemList();
                }
            }

            Edit.Mode = false;
        }
        
        public void ItemAdd()
        {
            var itemBtn = _MakePassFileSectionItemBtn(new PassFile.PwdSection.PwdItem());
            _sectionItemsList.Add(itemBtn);
            _viewElements.ItemScrollViewer!.ScrollToEnd();
        }

        private void ItemDelete(PassFileSectionItemBtn itemBtn)
        {
            _sectionItemsList.Remove(itemBtn);
        }
        
        private void ItemMove(PassFileSectionItemBtn itemBtn, int direction)
        {
            var index = _sectionItemsList.IndexOf(itemBtn);
            
            if (direction > 0 && index + direction < _sectionItemsList.Count 
                || direction < 0 && index + direction > -1)
            {
                _sectionItemsList.Move(index, index + direction);
            }
        }

        #endregion
    }
}