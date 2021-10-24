using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
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
            new Button { Content = "\uE74E", Command = SaveCommand }
        };

        private List<PassFile>? _passFiles;
        
        private string? _mode;
        public string? Mode
        {
            get => _mode;
            set => this.RaiseAndSetIfChanged(ref _mode, value);
        }

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
                PassFileSectionList = 
                    value == -1 ? null : _MakePassFileSectionList(PassFileList[value].PassFile);
            }
        }

        private int _passFilesSelectedSectionIndex = -1;
        public int PassFilesSelectedSectionIndex
        {
            get => _passFilesSelectedSectionIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _passFilesSelectedSectionIndex, value);
                PassFileSectionItemList =
                    value == -1 ? null : _MakePassFileSectionItemList(PassFileSectionList![value].Section);
            }
        }

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
            SaveCommand = ReactiveCommand.CreateFromTask(_SaveAsync);

            AddPassFileCommand = ReactiveCommand.CreateFromTask(_PassFileAddAsync);
            RenamePassFileCommand = ReactiveCommand.CreateFromTask<MenuItem>(_PassFileRenameAsync);
            ArchivePassFileCommand = ReactiveCommand.CreateFromTask<MenuItem>(_PassFileArchiveAsync);
            DeletePassFileCommand = ReactiveCommand.CreateFromTask<MenuItem>(_PassFileDeleteAsync);

            AddSectionCommand = ReactiveCommand.CreateFromTask(_SectionAddAsync);
            RenameSectionCommand = ReactiveCommand.CreateFromTask<MenuItem>(_SectionRenameAsync);
            DeleteSectionCommand = ReactiveCommand.CreateFromTask<MenuItem>(_SectionDeleteAsync);

            _passFilesBarBtn = this.WhenAnyValue(vm => vm.IsPassFilesBarOpened)
                .Select(isOpened => new BarBtn
                {
                    Width = isOpened ? 190 : 40,
                    Content = isOpened ? Resources.STORAGE__PASSFILES_TITLE : "\uE72b\uE72a",
                    FontFamily = isOpened ? FontFamilies.Default : FontFamilies.SegoeMdl2,
                    FontSize = isOpened ? 18 : 14
                })
                .ToProperty(this, vm => vm.PassFilesBarBtn);
            
            _isSectionsBarVisible = this.WhenAnyValue(vm => vm.PassFileSectionList)
                .Select(arr => arr is not null)
                .ToProperty(this, vm => vm.IsSectionsBarVisible);
            
            _isItemsBarVisible = this.WhenAnyValue(vm => vm.PassFileSectionItemList)
                .Select(arr => arr is not null)
                .ToProperty(this, vm => vm.IsItemsBarVisible);

            _passFiles = null;
            _passFileList = _MakePassFileList();

            this.WhenNavigatedToObservable()
                .InvokeCommand(ReactiveCommand.CreateFromTask(_LoadPassFilesAsync));
        }

        private PassFileBtn[] _MakePassFileList()
            => (_passFiles ?? new List<PassFile>()).Select((passFile, i) => new PassFileBtn(passFile, i)).ToArray();
        
        private static PassFileSectionBtn[] _MakePassFileSectionList(PassFile passFile)
            => (passFile.Data ?? new List<PassFile.Section>()).Select((section, i) => new PassFileSectionBtn(section, i)).ToArray();

        private static PassFileSectionItemBtn[] _MakePassFileSectionItemList(PassFile.Section section)
            => (section.Items ?? new List<PassFile.Section.Item>()).Select((item, i) => new PassFileSectionItemBtn(item, i)).ToArray();

        public override void Navigate()
        {
            if (AppConfig.Current.User is null)
            {
                FakeNavigated();
                NavigateTo<AuthRequiredViewModel>();
            }
            else
            {
                base.Navigate();
            }
        }

        public override Task RefreshAsync()
        {
            Navigate();
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