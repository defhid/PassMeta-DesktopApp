using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using DynamicData.Binding;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.Models.Storage;
using PassMeta.DesktopApp.Ui.ViewModels.Base;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels.Storage
{
    public partial class StorageViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/storage";

        public override ContentControl[] RightBarButtons => new ContentControl[]
        {
            new Button { Content = "\uE74E", Command = ReactiveCommand.CreateFromTask(SaveAsync) }
        };

        private static List<PassFile>? _passFiles;
        private static int? _userId;
        private static string? _mode;

        #region Texts
        
        public string? Mode
        {
            get => _mode;
            set => this.RaiseAndSetIfChanged(ref _mode, value);
        }
        
        private readonly ObservableAsPropertyHelper<bool> _isNoSectionsTextVisible;
        public bool IsNoSectionsTextVisible => _isNoSectionsTextVisible.Value;
        
        private readonly ObservableAsPropertyHelper<bool> _isNoItemsTextVisible;
        public bool IsNoItemsTextVisible => _isNoItemsTextVisible.Value;
        
        #endregion

        #region Lists

        private PassFileBtn[] _passFileList;
        public PassFileBtn[] PassFileList
        {
            get => _passFileList;
            private set {
                this.RaiseAndSetIfChanged(ref _passFileList, value);
                PassFileSectionList = null;
            }
        }

        private PassFileSectionBtn[]? _passFileSectionList;
        public PassFileSectionBtn[]? PassFileSectionList
        {
            get => _passFileSectionList;
            private set
            {
                this.RaiseAndSetIfChanged(ref _passFileSectionList, value);
                PassFileSectionItemList = null;
            }
        }
        
        private PassFileSectionItemBtn[]? _passFileSectionItemList;
        public PassFileSectionItemBtn[]? PassFileSectionItemList
        {
            get => _passFileSectionItemList;
            private set => this.RaiseAndSetIfChanged(ref _passFileSectionItemList, value);
        }
        
        #endregion

        #region Selection
        
        private int _passFilesSelectedIndex = -1;
        public int PassFilesSelectedIndex
        {
            get => _passFilesSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _passFilesSelectedIndex, value);
                _SetPassFileSectionList();
            }
        }

        private int _passFilesSelectedSectionIndex = -1;
        public int PassFilesSelectedSectionIndex
        {
            get => _passFilesSelectedSectionIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _passFilesSelectedSectionIndex, value);
                _SetPassFileSectionItemList();
            }
        }

        public PassFile? SelectedPassFile =>
            _passFilesSelectedIndex == -1 ? null : _passFileList[_passFilesSelectedIndex].PassFile;
        
        public PassFile.Section? SelectedSection =>
            _passFilesSelectedSectionIndex == -1 ? null : _passFileSectionList![_passFilesSelectedSectionIndex].Section;

        #endregion

        #region Navigation

        private bool _isPassFilesBarOpened;
        public bool IsPassFilesBarOpened
        {
            get => _isPassFilesBarOpened;
            set => this.RaiseAndSetIfChanged(ref _isPassFilesBarOpened, value);
        }

        private readonly ObservableAsPropertyHelper<BarBtn> _passFilesBarBtn;
        public BarBtn PassFilesBarBtn => _passFilesBarBtn.Value;

        private readonly ObservableAsPropertyHelper<bool> _isSectionsBarVisible;
        public bool IsSectionsBarVisible => _isSectionsBarVisible.Value;

        private readonly ObservableAsPropertyHelper<bool> _isItemsBarVisible;
        public bool IsItemsBarVisible => _isItemsBarVisible.Value;
        
        #endregion

        public StorageViewModel(IScreen hostScreen) : base(hostScreen)
        {
            _passFileList = _MakePassFileList();

            _isSectionsBarVisible = this.WhenValueChanged(vm => vm.PassFileSectionList)
                .Select(arr => arr is not null)
                .ToProperty(this, vm => vm.IsSectionsBarVisible);
            
            _isItemsBarVisible = this.WhenValueChanged(vm => vm.PassFileSectionItemList)
                .Select(arr => arr is not null)
                .ToProperty(this, vm => vm.IsItemsBarVisible);
            
            _isNoSectionsTextVisible = this.WhenValueChanged(vm => vm.PassFileSectionList)
                .Select(arr => arr?.Any() is false)
                .ToProperty(this, vm => vm.IsNoSectionsTextVisible);
            
            _isNoItemsTextVisible = this.WhenValueChanged(vm => vm.PassFileSectionItemList)
                .Select(arr => arr?.Any() is false)
                .ToProperty(this, vm => vm.IsNoItemsTextVisible);
            
            _passFilesBarBtn = this.WhenValueChanged(vm => vm.IsPassFilesBarOpened)
                .Select(isOpened => new BarBtn
                {
                    Width = isOpened ? 190 : 40,
                    Content = isOpened ? Resources.STORAGE__PASSFILES_TITLE : "\uE72b\uE72a",
                    FontFamily = isOpened ? FontFamilies.Default : FontFamilies.SegoeMdl2,
                    FontSize = isOpened ? 18 : 14
                })
                .ToProperty(this, vm => vm.PassFilesBarBtn);
            
            this.WhenValueChanged(vm => vm.IsPassFilesBarOpened)
                .Subscribe(isOpened =>
                {
                    foreach (var passFileBtn in PassFileList)
                    {
                        passFileBtn.ShortMode = !isOpened;
                    }
                });
            
            this.WhenNavigatedToObservable()
                .InvokeCommand(ReactiveCommand.CreateFromTask(_LoadPassFilesAsync));
        }

        private static PassFileBtn[] _MakePassFileList()
            => (_passFiles ?? new List<PassFile>()).Select((passFile, i) => new PassFileBtn(passFile, i)).ToArray();

        private void _SetPassFileSectionList()
        {
            var passFile = SelectedPassFile;
            if (passFile is null) PassFileSectionList = null;
            else if (passFile.Data is null) PassFileSectionList = Array.Empty<PassFileSectionBtn>();
            else PassFileSectionList = 
                passFile.Data.Select((section, i) => new PassFileSectionBtn(section, i)).ToArray();
        }

        private void _SetPassFileSectionItemList()
        {
            var section = SelectedSection;
            if (section is null) PassFileSectionItemList = null;
            else if (section.Items is null) PassFileSectionItemList = Array.Empty<PassFileSectionItemBtn>();
            else PassFileSectionItemList = 
                section.Items.Select((item, i) => new PassFileSectionItemBtn(item, i)).ToArray();
        }

        public override void Navigate()
        {
            if (AppConfig.Current.User is null)
            {
                FakeNavigated();
                NavigateTo<AuthRequiredViewModel>(typeof(StorageViewModel));
            }
            else
            {
                base.Navigate();
            }
        }

        public override Task RefreshAsync()
        {
            Navigate();
            
            _userId = null;
            _passFiles = null;
            
            return _LoadPassFilesAsync();
        } 
    }

    public class BarBtn
    {
        public double Width { get; set; }
        public string? Content { get; set; }
        public FontFamily? FontFamily { get; set; }
        public double FontSize { get; set; }
    }
}