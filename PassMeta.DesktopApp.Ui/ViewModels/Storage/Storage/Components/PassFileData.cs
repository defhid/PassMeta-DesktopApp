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
    using Core.Utils;
    using Models;
    using ReactiveUI;
    using Splat;
    using Utils.Comparers;
    using Views.Main;

    public class PassFileData : ReactiveObject
    {
        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;
        
        public IEnumerable<ContentControl> RightBarButtons => new ContentControl[]
        {
            new Button
            {
                Content = "\uE70F",
                
                Command = ReactiveCommand.Create(ItemsEdit),
                [!Button.IsVisibleProperty] = this.WhenAnyValue(vm => vm.SectionItemsList, vm => vm.EditMode)
                    .Select(x => x.Item1 is not null && !x.Item2)
                    .ToBinding()
            },
            new Button
            {
                Content = "\uE711", 
                Command = ReactiveCommand.Create(ItemsDiscardChanges),
                [!Button.IsVisibleProperty] = this.WhenAnyValue(vm => vm.SectionItemsList, vm => vm.EditMode)
                    .Select(x => x.Item1 is not null && x.Item2)
                    .ToBinding()
            },
            new Button
            {
                Content = "\uE8FB", 
                Command = ReactiveCommand.Create(ItemsApplyChanges),
                [!Button.IsVisibleProperty] = this.WhenAnyValue(vm => vm.SectionItemsList, vm => vm.EditMode)
                    .Select(x => x.Item1 is not null && x.Item2)
                    .ToBinding()
            },
            new Button
            {
                Content = "\uE74D", 
                Command = ReactiveCommand.Create(SectionDeleteAsync),
                [!Button.IsVisibleProperty] = this.WhenAnyValue(vm => vm.SectionItemsList, vm => vm.EditMode)
                    .Select(x => x.Item1 is not null && !x.Item2)
                    .ToBinding()
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
        public bool IsNoItemsTextVisible => !_sectionItemsList.Any() && !_editMode;

        private int _selectedSectionIndex = -1;
        public int SelectedSectionIndex
        {
            get => _selectedSectionIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedSectionIndex, value);
                _UpdatePassFileSectionItemList();
            }
        }
        
        public PassFileSectionBtn? SelectedSectionBtn =>
            _selectedSectionIndex < 0 ? null : _sectionsList[_selectedSectionIndex];
        
        public PassFile.Section? SelectedSection =>
            _selectedSectionIndex < 0 ? null : _sectionsList[_selectedSectionIndex].Section;

        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        private bool _editMode;
        public bool EditMode
        {
            get => _editMode;
            set => this.RaiseAndSetIfChanged(ref _editMode, value);
        }

        private readonly IObservable<bool> _editModeObservable;

        private int _searching;

        private readonly PassFileItemPath _lastPassFileItemPath;

        public PassFileData(PassFileItemPath lastPassFileItemPath)
        {
            _lastPassFileItemPath = lastPassFileItemPath;
            
            this.WhenAnyValue(vm => vm.SectionsList)
                .Subscribe(_ => _UpdatePassFileSectionItemList());
            
            IsSectionsBarVisible = this.WhenAnyValue(vm => vm.SectionsList)
                .Select(sections => sections is not null);

            IsItemsBarVisible = this.WhenAnyValue(vm => vm.SectionItemsList)
                .Select(items => items is not null);
            
            _sectionsList.CollectionChanged += (_, _) => this.RaisePropertyChanged(nameof(IsNoSectionsTextVisible));
            _sectionItemsList.CollectionChanged += (_, _) => this.RaisePropertyChanged(nameof(IsNoItemsTextVisible));
            
            _editModeObservable = this.WhenAnyValue(vm => vm.EditMode);

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

        private PassFileSectionBtn _MakePassFileSectionBtn(PassFile.Section section) 
            => new(section);
        
        private PassFileSectionItemBtn _MakePassFileSectionItemBtn(PassFile.Section.Item item) 
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
            
            var list = PassFile?.Data;
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

        private async Task SectionAddAsync()
        {
            var askName = await _dialogService.AskStringAsync(Resources.STORAGE__ASK_SECTION_NAME);
            if (askName.Bad || askName.Data == string.Empty) return;
            
            using var preloader = MainWindow.Current!.StartPreloader();
            var passFile = _passFile!;
            var section = new PassFile.Section { Name = askName.Data! };
            
            var result = PassFileLocalManager.UpdateDataSelectively(passFile, data => data.Add(section.Copy()));
            if (result.Ok)
            {
                passFile.Data!.Add(section);
                _UpdatePassFileSectionList(true);
                SelectedSectionIndex = _sectionsList.FindIndex(btn => btn.Section.Id == section.Id);
            }
            else
            {
                _dialogService.ShowError(result.Message!);
            }
        }

        public async Task SectionRenameAsync()  // TODO implement UI input
        {
            var passFile = _passFile!;
            var section = SelectedSection!;

            var askName = await _dialogService.AskStringAsync(Resources.STORAGE__ASK_SECTION_NAME, defaultValue: section.Name);
            if (askName.Bad || askName.Data == string.Empty || askName.Data == section.Name) return;
            
            using var preloader = MainWindow.Current!.StartPreloader();
            var name = askName.Data!;

            var result = PassFileLocalManager.UpdateDataSelectively(passFile, 
                data => data.First(s => s.Id == section.Id).Name = name);

            if (result.Ok)
            {
                passFile.Data!.First(s => s.Id == section.Id).Name = name;
                SelectedSectionBtn!.Refresh();
            }
            else
            {
                _dialogService.ShowError(result.Message!);
            }
        }
        
        public async Task SectionDeleteAsync()
        {
            var passFile = _passFile!;
            var section = SelectedSection!;
            
            var confirm = await _dialogService.ConfirmAsync(string.Format(Resources.STORAGE__CONFIRM_DELETE_SECTION, section.Name));
            if (confirm.Bad) return;

            using var preloader = MainWindow.Current!.StartPreloader();

            var result = PassFileLocalManager.UpdateDataSelectively(passFile, 
                data => data.RemoveAll(s => s.Id == section.Id));

            if (result.Ok)
            {
                passFile.Data!.RemoveAll(s => s.Id == section.Id);
                var index = SelectedSectionIndex;
                _sectionsList.RemoveAt(index);
                SelectedSectionIndex = Math.Min(index, _sectionsList.Count);
            }
            else
            {
                _dialogService.ShowError(result.Message!);
            }
        }
        
        #endregion

        #region Items

        private void ItemsEdit()
        {
            EditMode = true;
        }

        private void ItemsApplyChanges()
        {
            // TODO
            EditMode = false;
        }

        private void ItemsDiscardChanges()
        {
            _UpdatePassFileSectionItemList();
            EditMode = false;
        }
        
        private void ItemAdd()
        {
            var itemBtn = _MakePassFileSectionItemBtn(new PassFile.Section.Item());
            _sectionItemsList.Add(itemBtn);
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