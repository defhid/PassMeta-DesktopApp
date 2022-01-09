namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Avalonia;
    using Avalonia.Controls;
    using Base;
    using Common;
    using Common.Models.Entities;
    using Components;
    using Constants;
    using Core.Utils;
    using Models;
    using ReactiveUI;
    using ViewModels.Components;
    using AppContext = Core.Utils.AppContext;

    public partial class StorageViewModel : ViewModelPage
    {
        private static bool _loaded;
        private static readonly PassFileItemPath LastItemPath = new();
        
        public override ContentControl[] RightBarButtons => new ContentControl[]
        {
            new Button
            {
                Content = "\uE74E", 
                Command = ReactiveCommand.CreateFromTask(SaveAsync),
                [!Button.IsVisibleProperty] = this.WhenAnyValue(vm => vm.SelectedData.EditMode)
                    .Select(editMode => !editMode)
                    .ToBinding()
            }
        }.Concat(SelectedData.RightBarButtons).ToArray();

        #region Passfile list

        private ObservableCollection<PassFileBtn> _passFileList = new();
        public ObservableCollection<PassFileBtn> PassFileList
        {
            get => _passFileList;
            private set {
                this.RaiseAndSetIfChanged(ref _passFileList, value);
                SelectedData.PassFile = null;
            }
        }

        private int _passFilesSelectedIndex = -1;
        private int _passFilesPrevSelectedIndex = -1;
        public int PassFilesSelectedIndex
        {
            get => _passFilesSelectedIndex;
            set
            {
                _passFilesPrevSelectedIndex = _passFilesSelectedIndex;
                this.RaiseAndSetIfChanged(ref _passFilesSelectedIndex, value);
                SelectedData.PassFile = value >= 0 ? _passFileList[value].PassFile : null;
            }
        }
        
        public PassFileBtn? SelectedPassFileBtn =>
            _passFilesSelectedIndex == -1 ? null : _passFileList[_passFilesSelectedIndex];

        public PassFile? SelectedPassFile =>
            _passFilesSelectedIndex == -1 ? null : _passFileList[_passFilesSelectedIndex].PassFile;

        #endregion
        
        public PassFileData SelectedData { get; }

        #region Layout
        
        private readonly IObservable<bool> _passFileBarShortMode;

        private bool _isPassFilesBarOpened;
        public bool IsPassFilesBarOpened
        {
            get => _isPassFilesBarOpened;
            set => this.RaiseAndSetIfChanged(ref _isPassFilesBarOpened, value);
        }

        public BtnState PassFilesBarBtn { get; }
        
        public IObservable<LayoutState> LayoutState { get; }

        #endregion
        
        public StorageViewModel(IScreen hostScreen) : base(hostScreen)
        {
            var passFileBarMode = this.WhenAnyValue(vm => vm.IsPassFilesBarOpened);

            PassFilesBarBtn = new BtnState
            {
                ContentObservable = passFileBarMode.Select(isOpened => isOpened ? Resources.STORAGE__PASSFILES_TITLE : "\uE72b\uE72a"),
                FontFamilyObservable = passFileBarMode.Select(isOpened => isOpened ? FontFamilies.Default : FontFamilies.SegoeMdl2),
                FontSizeObservable = passFileBarMode.Select(isOpened => isOpened ? 18d : 14d)
            };

            LayoutState = this.WhenAnyValue(vm => vm.PassFilesSelectedIndex,
                    vm => vm.SelectedData.SelectedSectionIndex)
                .Select(x => x.Item1 < 0
                    ? InitLayoutState
                    : x.Item2 < 0
                        ? AfterPassFileSelectionLayoutState
                        : AfterSectionSelectionLayoutState);

            _passFileBarShortMode = passFileBarMode.Select(isOpened => !isOpened);

            this.WhenAnyValue(vm => vm.SelectedData.SelectedSectionIndex)
                .Subscribe(index => IsPassFilesBarOpened = index == -1);

            this.WhenAnyValue(vm => vm.PassFilesSelectedIndex)
                .InvokeCommand(ReactiveCommand.CreateFromTask<int>(_DecryptIfRequiredAsync));
            
            var lastItemPath = LastItemPath.Copy();

            SelectedData = new PassFileData(LastItemPath);
            
            this.WhenNavigatedToObservable()
                .InvokeCommand(ReactiveCommand.CreateFromTask(() => _LoadPassFilesAsync(lastItemPath)));
        }

        public override void Navigate()
        {
            if (AppContext.Current.User is null)
            {
                FakeNavigated();
                NavigateTo<AuthRequiredViewModel>(typeof(StorageViewModel));
            }
            else
            {
                base.Navigate();
            }
        }

        public override async Task RefreshAsync()
        {
            if (AppContext.Current.User is null)
            {
                NavigateTo<AuthRequiredViewModel>(typeof(StorageViewModel));
            }

            if (PassFileLocalManager.AnyCurrentChanged)
            {
                var confirm = await _dialogService.ConfirmAsync(Resources.STORAGE__CONFIRM_REFRESH_FROM_SERVER);
                if (confirm.Bad) return;
            }

            await _LoadPassFilesAsync(LastItemPath.Copy());
        }

        #region Layout states

        private static readonly LayoutState InitLayoutState = new()
        {
            PassFilesPaneWidth = 250d,
            SectionsListMargin = new Thickness(0)
        };
        
        private static readonly LayoutState AfterPassFileSelectionLayoutState = new()
        {
            PassFilesPaneWidth = 250d,
            SectionsListMargin = new Thickness(0, 0, -100, 0)
        };
        
        private static readonly LayoutState AfterSectionSelectionLayoutState = new()
        {
            PassFilesPaneWidth = 200d,
            SectionsListMargin = new Thickness(0)
        };

        #endregion
    }
}