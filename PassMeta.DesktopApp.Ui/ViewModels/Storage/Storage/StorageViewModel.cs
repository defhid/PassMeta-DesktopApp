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
    using Common.Enums;
    using Common.Interfaces.Services;
    using Common.Interfaces.Services.PassFile;
    using Common.Models.Entities;
    using Common.Utils.Extensions;
    using Components;
    using Constants;
    using Core;
    using Core.Utils;
    using Models;
    using ReactiveUI;
    using Utils.Comparers;
    using Utils.Extensions;
    using ViewModels.Components;
    using Views.Main;
    
    using AppContext = Core.Utils.AppContext;

    public class StorageViewModel : PageViewModel
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
                [!Button.IsEnabledProperty] = PassFileManager.AnyCurrentChangedSource
                    .Select(state => state)
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
        
        private readonly IPassFileService _passFileService = EnvironmentContainer.Resolve<IPassFileService>();
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();

        public StorageViewModel(IScreen hostScreen) : base(hostScreen)
        {
            PassFileBarExpander = new PassFileBarExpander();
            
            PassFilesBarBtn = new BtnState
            {
                ContentObservable = PassFileBarExpander.IsOpenedObservable.Select(isOpened => isOpened 
                    ? Resources.STORAGE__TITLE 
                    : "\uE92D"),
                FontFamilyObservable = PassFileBarExpander.IsOpenedObservable.Select(isOpened => isOpened 
                    ? FontFamilies.Default 
                    : FontFamilies.SegoeMdl2),
                FontSizeObservable = PassFileBarExpander.IsOpenedObservable.Select(isOpened => isOpened 
                    ? 18d 
                    : 22d),
                RotationAngleObservable = PassFileBarExpander.IsOpenedObservable.Select(isOpened => isOpened 
                    ? 0 
                    : 45),
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
            SelectedData.PassFileCanBeChanged += (_, _) => SelectedPassFileBtn?.RefreshState();
            
            SelectedData.WhenAnyValue(vm => vm.SelectedSectionIndex)
                .Subscribe(index => PassFileBarExpander.TryExecuteAutoExpanding(index == -1));
            
            this.WhenAnyValue(vm => vm.PassFileList)
                .Subscribe(_ => SelectedData.PassFile = null);
            
            this.WhenAnyValue(vm => vm.PassFilesSelectedIndex)
                .InvokeCommand(ReactiveCommand.CreateFromTask<int>(DecryptIfRequiredAndSetSectionsAsync));

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
                .InvokeCommand(ReactiveCommand.CreateFromTask(() => LoadPassFilesAsync(lastItemPath)));
        }

        public override void TryNavigate()
        {
            if (AppContext.Current.User is null)
            {
                FakeNavigated();
                TryNavigateTo<AuthRequiredViewModel>(typeof(StorageViewModel));
            }
            else
            {
                base.TryNavigate();
            }
        }
        
        protected override async Task<bool> OnCloseAsync()
        {
            if (!SelectedData.Edit.Mode) return true;
            var confirm = await _dialogService.ConfirmAsync(Resources.STORAGE__CONFIRM_SECTION_RESET);
            return confirm.Ok;
        }

        public override async Task RefreshAsync()
        {
            if (AppContext.Current.User is null)
            {
                TryNavigateTo<AuthRequiredViewModel>(typeof(StorageViewModel));
            }

            if (PassFileManager.AnyCurrentChanged)
            {
                var confirm = await _dialogService.ConfirmAsync(Resources.STORAGE__CONFIRM_ROLLBACK);
                if (confirm.Bad) return;
                
                PassFileManager.Rollback();
            }

            _loaded = false;
            await LoadPassFilesAsync(LastItemPath.Copy());
        }

        private PassFileBtn _MakePassFileBtn(PassFile passFile)
        {
            var passFileBtn = new PassFileBtn(passFile, PassFileBarExpander.ShortModeObservable);
            passFileBtn.PassFileChanged += (sender, ev) =>
            {
                if (ev.PassFileNew != null) return;
            
                PassFileList.Remove((PassFileBtn)sender!);
            };
            return passFileBtn;
        }

        private async Task LoadPassFilesAsync(PassFileItemPath lastItemPath)
        {
            using var preloader = MainWindow.Current!.StartPreloader();

            if (!_loaded)
            {
                await _passFileService.RefreshLocalPassFilesAsync(PassFileType.Pwd);
                _loaded = true;
            }

            UpdatePassFileList();

            if (lastItemPath.PassFileId is not null)
            {
                var index = _passFileList.FindIndex(btn => btn.PassFile!.Id == lastItemPath.PassFileId.Value);
                if (index >= 0)
                {
                    if (_passFileList[index].PassFile?.PassPhrase is not null)
                    {
                        PassFilesSelectedIndex = index;
                        if (lastItemPath.PassFileSectionId is not null)
                        {
                            SelectedData.SelectedSectionIndex =
                                SelectedData.SectionsList!.FindIndex(btn => btn.Section.Id == lastItemPath.PassFileSectionId);
                        }
                    }
                }
            }

            PassFileBarExpander.IsOpened = true;
        }

        private void UpdatePassFileList()
        {
            var list = PassFileManager.GetCurrentList(PassFileType.Pwd);
            list.Sort(new PassFileComparer());

            var localCreated = list.Where(pf => pf.LocalCreated);
            var localChanged = list.Where(pf => pf.LocalChanged); 
            var unchanged = list.Where(pf => pf.LocalNotChanged);
            var localDeleted = list.Where(pf => pf.LocalDeleted);

            PassFileList = new ObservableCollection<PassFileBtn>(
                localCreated.Concat(localChanged).Concat(unchanged).Concat(localDeleted).Select(_MakePassFileBtn));
        }

        private async Task DecryptIfRequiredAndSetSectionsAsync(int _)
        {
            var passFile = SelectedPassFile;
            if (passFile is null || passFile.DataPwd is not null)
            {
                SelectedData.PassFile = passFile;
                return;
            }

            if (!passFile.LocalDeleted)
            {
                using var preloader = MainWindow.Current!.StartPreloader();

                var result = await passFile.LoadIfRequiredAndDecryptAsync(_dialogService);
                if (result.Ok)
                {
                    SelectedData.PassFile = passFile;
                    return;
                }
            }

            PassFilesSelectedIndex = _passFilesPrevSelectedIndex;
        }

        private async Task SaveAsync()
        {
            using (MainWindow.Current!.StartPreloader())
            {
                await _passFileService.ApplyPassFileLocalChangesAsync(PassFileType.Pwd);
            }

            await LoadPassFilesAsync(LastItemPath.Copy());
        }

        public async Task PassFileAddAsync()
        {
            using var preloader = MainWindow.Current!.StartPreloader();
            
            var askPassPhrase = await _dialogService.AskPasswordAsync(Resources.STORAGE__ASK_PASSPHRASE_FOR_NEW_PASSFILE);
            if (askPassPhrase.Bad || askPassPhrase.Data == string.Empty) return;
            
            var passFile = PassFileManager.CreateNew(PassFileType.Pwd, askPassPhrase.Data!);
            var passFileBtn = _MakePassFileBtn(passFile);
            
            PassFileList.Insert(0, passFileBtn);
            PassFilesSelectedIndex = 0;
             
            await passFileBtn.OpenAsync();
        }

        public async Task PassFileOpenAsync()
        {
            await SelectedPassFileBtn!.OpenAsync();
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