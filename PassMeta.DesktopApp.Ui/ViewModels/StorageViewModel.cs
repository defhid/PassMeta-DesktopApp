using System;
using System.Collections.Generic;
using System.Linq;
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

        public StorageViewModel(IScreen hostScreen) : base(hostScreen)
        {
        }

        public void SetPassFileList(IEnumerable<PassFile> passFiles, int activeId = 0, bool shortMode = false)
        {
            PassFiles = passFiles.Select(pf => 
                new PassFileBtn(pf, shortMode, pf.Id == activeId)).ToArray();
        }
        
        public void SetPassFileSectionList(int activeIndex, bool shortMode = false)
        {
            PassFileSections = _passFiles.First(pf => pf.Active)
                .Sections.Select((s, i) => 
                    new PassFileSectionBtn(s, i, shortMode, i == activeIndex)).ToArray();
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
    }
}