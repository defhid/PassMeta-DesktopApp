using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.Models.Storage;
using PassMeta.DesktopApp.Ui.ViewModels.Base;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class StorageViewModel : ViewModelPage
    {
        public override string UrlPathSegment => "/storage";

        public override ContentControl[] RightBarButtons => new [] { _MakeSaveBtn() };

        #region Lists

        private PassFileBtn[] _passFiles = Array.Empty<PassFileBtn>();
        public PassFileBtn[] PassFiles
        {
            get => _passFiles;
            private set {
                this.RaiseAndSetIfChanged(ref _passFiles, value);
                
                var active = value.FirstOrDefault(pf => pf.Active);
                if (active is null)
                {
                    IsSectionsBarVisible = false;
                    PassFileSections = Array.Empty<PassFileSectionBtn>();
                }
                else
                {
                    IsSectionsBarVisible = true;
                    PassFileSections = active.Sections.Select((s, i) => new PassFileSectionBtn(s, i)).ToArray();
                }
            }
        }

        private PassFileSectionBtn[] _passFileSections = Array.Empty<PassFileSectionBtn>();
        public PassFileSectionBtn[] PassFileSections
        {
            get => _passFileSections;
            private set
            {
                this.RaiseAndSetIfChanged(ref _passFileSections, value);
                
                var active = value.FirstOrDefault(pf => pf.Active);
                if (active is null)
                {
                    IsItemsBarVisible = false;
                    PassFileSectionItems = Array.Empty<PassFileSectionItemBtn>();
                }
                else
                {
                    IsItemsBarVisible = true;
                    PassFileSectionItems = active.Items.Select((item, i) => new PassFileSectionItemBtn(item, i)).ToArray();
                }
            }
        }
        
        private PassFileSectionItemBtn[] _passFileSectionItems = Array.Empty<PassFileSectionItemBtn>();
        public PassFileSectionItemBtn[] PassFileSectionItems
        {
            get => _passFileSectionItems;
            private set => this.RaiseAndSetIfChanged(ref _passFileSectionItems, value);
        }

        private int? _passFilesSelectedIndex;
        public int? PassFilesSelectedIndex
        {
            get => _passFilesSelectedIndex;
            set
            {
                // TODO
                this.RaiseAndSetIfChanged(ref _passFilesSelectedIndex, value);
            }
        }
        
        #endregion

        #region Navigation

        private BarBtn _passFilesBarBtn = new();
        public BarBtn PassFilesBarBtn { 
            get => _passFilesBarBtn;
            private set => this.RaiseAndSetIfChanged(ref _passFilesBarBtn, value);
        }

        private bool _isPassFilesBarOpened;
        public bool IsPassFilesBarOpened
        {
            get => _isPassFilesBarOpened;
            set => this.RaiseAndSetIfChanged(ref _isPassFilesBarOpened, value);
        }
        
        private bool _isSectionsBarVisible;
        public bool IsSectionsBarVisible
        {
            get => _isSectionsBarVisible;
            set => this.RaiseAndSetIfChanged(ref _isSectionsBarVisible, value);
        }

        private bool _isItemsBarVisible;
        public bool IsItemsBarVisible
        {
            get => _isItemsBarVisible;
            set => this.RaiseAndSetIfChanged(ref _isItemsBarVisible, value);
        }
        
        #endregion
        
        private string? _mode;
        public string? Mode
        {
            get => _mode;
            set => this.RaiseAndSetIfChanged(ref _mode, value);
        }

        public StorageViewModel(IScreen hostScreen) : base(hostScreen)
        {
            _SetPassFilesBarBtn(false);
        }

        public void SetPassFileList(IEnumerable<PassFile> passFiles, int activeId = 0, bool shortMode = false)
        {
            _SetPassFilesBarBtn(shortMode);
            
            PassFiles = passFiles.Select(pf => 
                new PassFileBtn(pf, shortMode, pf.Id == activeId)).ToArray();

            IsPassFilesBarOpened = !shortMode;
        }
        
        public void SetPassFileSectionList(int activeIndex)
        {
            PassFileSections = PassFiles.First(pf => pf.Active)
                .Sections.Select((s, i) => 
                    new PassFileSectionBtn(s, i, i == activeIndex)).ToArray();
        }
        
        public void SetPassFileSectionItemList(int activeIndex)
        {
            PassFileSectionItems = _passFileSections.First(s => s.Active)
                .Items.Select((item, i) => 
                    new PassFileSectionItemBtn(item, i, i == activeIndex)).ToArray();
        }

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

        private void _SetPassFilesBarBtn(bool shortMode)
        {
            PassFilesBarBtn = new BarBtn
            {
                Width = shortMode ? 40 : 190,
                Content = shortMode ? "\uE72b\uE72a" : Resources.STORAGE__PASSFILES_TITLE,
                FontFamily = shortMode ? "Segoe MDL2 Assets" : FontFamily.Default,
                FontSize = shortMode ? 14 : 18
            };
        }

        private ContentControl _MakeSaveBtn()
        {
            var btnSave = new Button { Content = "\uE74E" };
            btnSave.Click += (sender, ev) =>
            {  
                // TODO
            };
            return btnSave;
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