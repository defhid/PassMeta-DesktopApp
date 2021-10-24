using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using DynamicData;
using ReactiveUI;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Constants;

namespace PassMeta.DesktopApp.Ui.ViewModels.Main
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

        private ContentControl[]? _rightBarButtons;
        public ContentControl[]? RightBarButtons
        {
            get => _rightBarButtons;
            set => this.RaiseAndSetIfChanged(ref _rightBarButtons, value);
        }

        public MainWindowViewModel()
        {
            _mainPaneButtons = _mainPaneButtonsWhenClosed;
            _mainPaneButtonsActive = new [] { false, false, false, false };
            
            this.WhenAnyValue(vm => vm.IsMainPaneOpened)
                .Subscribe(isMainPaneOpened => MainPaneButtons = isMainPaneOpened 
                    ? _mainPaneButtonsWhenOpened
                    : _mainPaneButtonsWhenClosed);
        }
    }

#pragma warning disable 8618
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
            FontFamily = FontFamilies.Default,
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
            FontFamily = FontFamilies.SegoeMdl2,
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
#pragma warning restore 8618
}