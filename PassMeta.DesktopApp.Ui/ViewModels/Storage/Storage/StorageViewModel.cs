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
                [!Button.IsVisibleProperty] = SelectedData.Edit.WhenAnyValue(vm => vm.Mode)
                    .Select(editMode => !editMode)
                    .ToBinding(),
                [ToolTip.TipProperty] = Resources.STORAGE__RIGHT_BAR_TOOLTIP__SAVE,
                [ToolTip.PlacementProperty] = PlacementMode.Left
            }
        }.Concat(SelectedData.RightBarButtons).ToArray();

        #region Passfile list

        private ObservableCollection<PassFileBtn> _passFileList = new();
        public ObservableCollection<PassFileBtn> PassFileList
        {
            get => _passFileList;
            private set => this.RaiseAndSetIfChanged(ref _passFileList, value);
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
            }
        }
        
        public PassFileBtn? SelectedPassFileBtn =>
            _passFilesSelectedIndex == -1 ? null : _passFileList[_passFilesSelectedIndex];

        public PassFile? SelectedPassFile =>
            _passFilesSelectedIndex == -1 ? null : _passFileList[_passFilesSelectedIndex].PassFile;

        #endregion
        
        public PassFileData SelectedData { get; }

        public PassFileBarExpander PassFileBarExpander { get; }
        
        public BtnState PassFilesBarBtn { get; }
        
        public IObservable<LayoutState> LayoutState { get; }

        public readonly ViewElements ViewElements = new();
        
        public StorageViewModel(IScreen hostScreen) : base(hostScreen)
        {
            PassFileBarExpander = new PassFileBarExpander();
            
            PassFilesBarBtn = new BtnState
            {
                ContentObservable = PassFileBarExpander.IsOpenedObservable.Select(isOpened => isOpened 
                    ? Resources.STORAGE__TITLE 
                    : "\uE72b\uE72a"),
                FontFamilyObservable = PassFileBarExpander.IsOpenedObservable.Select(isOpened => isOpened 
                    ? FontFamilies.Default 
                    : FontFamilies.SegoeMdl2),
                FontSizeObservable = PassFileBarExpander.IsOpenedObservable.Select(isOpened => isOpened 
                    ? 18d 
                    : 14d)
            };

            LayoutState = this.WhenAnyValue(vm => vm.PassFilesSelectedIndex,
                    vm => vm.SelectedData.SelectedSectionIndex)
                .Select(x => x.Item1 < 0
                    ? InitLayoutState
                    : x.Item2 < 0
                        ? AfterPassFileSelectionLayoutState
                        : AfterSectionSelectionLayoutState);

            var lastItemPath = LastItemPath.Copy();

            SelectedData = new PassFileData(ViewElements, LastItemPath, PassFileBarExpander);
            
            SelectedData.WhenAnyValue(vm => vm.SelectedSectionIndex)
                .Subscribe(index => PassFileBarExpander.TryExecuteAutoExpanding(index == -1));
            
            this.WhenAnyValue(vm => vm.PassFileList)
                .Subscribe(_ => SelectedData.PassFile = null);
            
            this.WhenAnyValue(vm => vm.PassFilesSelectedIndex)
                .InvokeCommand(ReactiveCommand.CreateFromTask<int>(_DecryptIfRequiredAndSetSectionsAsync));

            SelectedData.WhenAnyValue(vm => vm.SelectedSectionIndex)
                .Subscribe(index =>
                {
                    if (index < 0)
                        PassFileBarExpander.AutoExpanding = true;
                });

            PassFileBarExpander.IsOpenedObservable
                .Subscribe(isOpened =>
                {
                    PassFileBarExpander.AutoExpanding = !(isOpened && _passFilesSelectedIndex >= 0);
                });

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

            if (PassFileManager.AnyCurrentChanged)
            {
                var confirm = await _dialogService.ConfirmAsync(Resources.STORAGE__CONFIRM_ROLLBACK);
                if (confirm.Bad) return;
                
                PassFileManager.Rollback();
            }

            _loaded = false;
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