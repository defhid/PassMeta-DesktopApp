using System;
using System.Collections.Generic;
using System.Linq;
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

        private PassFileBtn[] _passFiles = Array.Empty<PassFileBtn>();
        public PassFileBtn[] PassFiles
        {
            get => _passFiles;
            set {
                this.RaiseAndSetIfChanged(ref _passFiles, value);
                
                var active = value.FirstOrDefault(pf => pf.Active);
            
                PassFileSections = active is null 
                    ? Array.Empty<PassFileSectionBtn>()
                    : active.Sections.Select((s, i) => new PassFileSectionBtn(s, i)).ToArray();
            }
        }

        private PassFileSectionBtn[] _passFileSections = Array.Empty<PassFileSectionBtn>();
        public PassFileSectionBtn[] PassFileSections
        {
            get => _passFileSections;
            set
            {
                this.RaiseAndSetIfChanged(ref _passFileSections, value);
                
                var active = value.FirstOrDefault(pf => pf.Active);
            
                PassFileSectionItems = active is null 
                    ? Array.Empty<PassFileSectionItemBtn>()
                    : active.Items.Select((item, i) => new PassFileSectionItemBtn(item, i)).ToArray();
            }
        }
        
        private PassFileSectionItemBtn[] _passFileSectionItems = Array.Empty<PassFileSectionItemBtn>();
        public PassFileSectionItemBtn[] PassFileSectionItems
        {
            get => _passFileSectionItems;
            set => this.RaiseAndSetIfChanged(ref _passFileSectionItems, value);
        }

        private string _passFilesMode = "";

        public string? PassFilesMode
        {
            get => _passFilesMode;
            set => this.RaiseAndSetIfChanged(ref _passFilesMode, value ?? "");
        }

        private BarBtn _passFilesBarBtn = new();
        public BarBtn PassFilesBarBtn { 
            get => _passFilesBarBtn;
            set => this.RaiseAndSetIfChanged(ref _passFilesBarBtn, value);
        }

        private bool _isPassFilesBarOpened = false;
        public bool IsPassFilesBarOpened
        {
            get => _isPassFilesBarOpened;
            set => this.RaiseAndSetIfChanged(ref _isPassFilesBarOpened, value);
        }
        
        private bool _isSectionsBarVisible = false;
        public bool IsSectionsBarVisible
        {
            get => _isSectionsBarVisible;
            set => this.RaiseAndSetIfChanged(ref _isSectionsBarVisible, value);
        }

        public StorageViewModel(IScreen hostScreen) : base(hostScreen)
        {
            _SetPassFilesBarBtnWidth(false);
        }

        public void SetPassFileList(IEnumerable<PassFile> passFiles, int activeId = 0, bool shortMode = false)
        {
            _SetPassFilesBarBtnWidth(shortMode);
            
            PassFiles = passFiles.Select(pf => 
                new PassFileBtn(pf, shortMode, pf.Id == activeId)).ToArray();

            IsPassFilesBarOpened = !shortMode;
            IsSectionsBarVisible = activeId > 0;
        }
        
        public void SetPassFileSectionList(int activeIndex)
        {
            PassFileSections = _passFiles.First(pf => pf.Active)
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

        private void _SetPassFilesBarBtnWidth(bool shortMode)
        {
            PassFilesBarBtn = new BarBtn
            {
                Width = shortMode ? 40 : 240,
                Content = shortMode ? "\uE72b\uE72a" : Resources.STORAGE__PASSFILES_TITLE,
                FontFamily = shortMode ? "Segoe MDL2 Assets" : FontFamily.Default,
                FontSize = shortMode ? 14 : 18
            };
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