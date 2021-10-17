using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.Models.Storage;
using PassMeta.DesktopApp.Ui.ViewModels;
using PassMeta.DesktopApp.Ui.Views.Base;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views
{
    // ReSharper disable once UnusedType.Global
    public class StorageView : ViewPage<StorageViewModel>
    {
        private List<PassFile> _passFiles = new();

        private int _passFilesActiveId = 0;
        private bool _passFilesShortMode = false;

        private int _passFileSectionsActiveIndex = 0;
        private int _passFileSectionItemsActiveIndex = 0;
        
        public StorageView()
        {
            AvaloniaXamlLoader.Load(this);
            AttachedToLogicalTree += OnAttachedToLogicalTree;
        }
        
        public override Task RefreshAsync()
        {
            throw new NotImplementedException();
        }
        
        private async void OnAttachedToLogicalTree(object? sender, EventArgs e)
        {
            DataContext!.SetPassFileList(Array.Empty<PassFile>(), 0, true);
            
            if (AppConfig.Current.PassFilesKeyPhrase is null)
            {
                var passPhrase = await Locator.Current.GetService<IDialogService>()!
                    .AskString(Common.Resources.ASK__PASSPHRASE);
                if (passPhrase.Bad) return;

                AppConfig.Current.PassFilesKeyPhrase = passPhrase.Data;
            }
            
            var result = await Locator.Current.GetService<IPassFileService>()!.GetPassFileListAsync();
            if (result.Bad) return;
            
            _passFiles = result.Data!;
            DataContext!.Mode = result.Message;
            DataContext!.SetPassFileList(_passFiles);
        }
        
        private void PassFilesBarBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender!;
            _passFilesShortMode = !(button.IsChecked ?? false);
            _RefreshPassFileList();
        }
        
        #region Selection Change

        private void PassFiles_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var item = ((ListBox)sender!).SelectedItem as PassFileBtn;
            _passFilesActiveId = item?.Id ?? 0;
            
            _RefreshPassFileList();
        }
        
        private void PassFileSections_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            _passFileSectionsActiveIndex = ((PassFileSectionBtn)((ListBox)sender!).SelectedItem!).Index;
            
            Locator.Current.GetService<IDialogService>()!.ShowInfo(_passFileSectionsActiveIndex.ToString());
            
            _RefreshPassFileSectionsList();
        }
        
        private void PassFileSectionItems_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            _passFileSectionItemsActiveIndex = ((PassFileSectionItemBtn)((ListBox)sender!).SelectedItem!).Index;
            
            Locator.Current.GetService<IDialogService>()!.ShowInfo(_passFileSectionItemsActiveIndex.ToString());
            
            _RefreshPassFileSectionItemsList();
        }
        
        #endregion

        #region Context Menu
        
        private void PassFileContextMenuRename_OnClick(object? sender, RoutedEventArgs e)
        {
            
        }
        
        private void PassFileContextMenuArchive_OnClick(object? sender, RoutedEventArgs e)
        {
            
        }
        
        private void PassFileContextMenuDelete_OnClick(object? sender, RoutedEventArgs e)
        {
            
        }

        private void SectionContextMenuRename_OnClick(object? sender, RoutedEventArgs e)
        {
            
        }
        
        private void SectionContextMenuDelete_OnClick(object? sender, RoutedEventArgs e)
        {
            
        }

        #endregion
        
        private void _RefreshPassFileList() 
            => DataContext?.SetPassFileList(_passFiles, _passFilesActiveId, _passFilesShortMode);
        
        private void _RefreshPassFileSectionsList() 
            => DataContext?.SetPassFileSectionList(_passFileSectionsActiveIndex);
        
        private void _RefreshPassFileSectionItemsList() 
            => DataContext?.SetPassFileSectionItemList(_passFileSectionItemsActiveIndex);
    }
}