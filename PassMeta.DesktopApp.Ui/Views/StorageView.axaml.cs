using System;
using System.Collections.Generic;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models.Entities;
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
        private bool _passFileSectionsShortMode = false;
        
        private int _passFileSectionItemsActiveIndex = 0;
        
        public StorageView()
        {
            AvaloniaXamlLoader.Load(this);
            AttachedToLogicalTree += _OnAttachedToLogicalTree;
        }

        private void PassFileBtn_OnChecked(object? sender, RoutedEventArgs e)
        {
            _passFilesActiveId = (int)((ToggleButton)sender!).Tag!;
            
            Locator.Current.GetService<IDialogService>()!.ShowInfo(_passFilesActiveId.ToString());
            
            DataContext!.SetPassFileList(_passFiles, _passFilesActiveId, _passFilesShortMode);
        }
        
        private void PassFileSectionBtn_OnChecked(object? sender, RoutedEventArgs e)
        {
            _passFileSectionsActiveIndex = (int)((ToggleButton)sender!).Tag!;
            
            Locator.Current.GetService<IDialogService>()!.ShowInfo(_passFileSectionsActiveIndex.ToString());
            
            DataContext!.SetPassFileSectionList(_passFileSectionsActiveIndex, _passFileSectionsShortMode);
        }
        
        private void PassFileSectionItemBtn_OnChecked(object? sender, RoutedEventArgs e)
        {
            _passFileSectionItemsActiveIndex = (int)((ToggleButton)sender!).Tag!;
            
            Locator.Current.GetService<IDialogService>()!.ShowInfo(_passFileSectionItemsActiveIndex.ToString());
            
            DataContext!.SetPassFileSectionItemList(_passFileSectionItemsActiveIndex);
        }
        
        private async void _OnAttachedToLogicalTree(object? sender, EventArgs e)
        {
            var result = await Locator.Current.GetService<IPassFileService>()!.GetPassFileListAsync();
            if (result.Bad) return;
            
            _passFiles = result.Data;
            DataContext!.PassFilesMode = result.Message;
            DataContext!.SetPassFileList(_passFiles);
        }
    }
}