﻿using System;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using DynamicData;
using PassMeta.DesktopApp.Core;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IScreen
    {
        public RoutingState Router { get; } = new();
        
        private readonly MainPaneButtons _mainPaneButtonsWhenClosed = MainPaneButtons.WhenClosed;
        private readonly MainPaneButtons _mainPaneButtonsWhenOpened = MainPaneButtons.WhenOpened;
        
        private bool _isMainPaneOpened;
        public bool IsMainPaneOpened
        {
            get => _isMainPaneOpened;
            set => this.RaiseAndSetIfChanged(ref _isMainPaneOpened, value);
        }

        private MainPaneButtons _mainPaneButtons;
        public MainPaneButtons MainPaneButtons
        {
            get => _mainPaneButtons;
            set => this.RaiseAndSetIfChanged(ref _mainPaneButtons, value);
        }

        private bool[] _mainPaneButtonsActive;
        public bool[] MainPaneButtonsActive
        {
            get => _mainPaneButtonsActive;
            set => this.RaiseAndSetIfChanged(ref _mainPaneButtonsActive, value);
        }

        public int ActiveMainPaneButtonIndex
        {
            get => _mainPaneButtonsActive.IndexOf(true);
            set
            {
                var activeNew = new bool[MainPaneButtonsActive.Length];
                for (var i = 0; i < MainPaneButtonsActive.Length; ++i)
                {
                    activeNew[i] = i == value;
                }
                MainPaneButtonsActive = activeNew;
            }
        }

        public MainWindowViewModel()
        {
            _mainPaneButtons = _mainPaneButtonsWhenClosed;
            _mainPaneButtonsActive = new [] { false, false, false, false };
            
            this.WhenAnyValue(vm => vm.IsMainPaneOpened)
                .Subscribe(_ => MainPaneButtons = _isMainPaneOpened 
                    ? _mainPaneButtonsWhenOpened
                    : _mainPaneButtonsWhenClosed);
        }
    }

    public class MainPaneButtons
    {
        public int Width { get; set; }
        
        public int Spacing { get; set; }
        
        public Thickness Padding { get; set; }

        public HorizontalAlignment HorizontalContentAlignment { get; set; }
        
        public FontFamily FontFamily { get; set; }
        
        public int FontSize { get; set; }
        
        public string[] Content { get; set; }

        public static MainPaneButtons WhenOpened => new()
        {
            Width = 180,
            Spacing = 8,
            Padding = Thickness.Parse("10 0 5 0"),
            HorizontalContentAlignment = HorizontalAlignment.Left,
            FontFamily = FontFamily.Default,
            FontSize = 20,
            Content = new[]
            {
                Resources.MAIN_MENU__ACCOUNT_BTN,
                Resources.MAIN_MENU__STORAGE_BTN,
                Resources.MAIN_MENU__GENERATOR_BTN,
                Resources.MAIN_MENU__SETTINGS_BTN,
            },
        };
        
        public static MainPaneButtons WhenClosed => new()
        {
            Width = 40,
            Spacing = 8,
            Padding = Thickness.Parse("0 0 0 0"),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            FontFamily = "Segoe MDL2 Assets",
            FontSize = 24,
            Content = new[]
            {
                "\uE77b",
                "\uE8F1",
                "\uEA80",
                "\uE713",
            },
        };
    }
}