using System;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using ReactiveUI;

namespace DesktopApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IScreen
    {
        private bool _isMainPaneOpened;
        public bool IsMainPaneOpened
        {
            get => _isMainPaneOpened;
            set => this.RaiseAndSetIfChanged(ref _isMainPaneOpened, value);
        }

        private MainPaneButtons _mainPaneButtons = MainPaneButtons.WhenClosed;
        public MainPaneButtons MainPaneButtons
        {
            get => _mainPaneButtons;
            set => this.RaiseAndSetIfChanged(ref _mainPaneButtons, value);
        }
        
        private void _MainPaneOpenedOrClosed()
        {
            // Colors.LightSkyBlue
            MainPaneButtons = _isMainPaneOpened 
                ? MainPaneButtons.WhenOpened 
                : MainPaneButtons.WhenClosed;
        }

        private bool[] _mainPaneButtonsActive = { true, false, false, false };
        public bool[] MainPaneButtonsActive
        {
            get => _mainPaneButtonsActive;
            set => this.RaiseAndSetIfChanged(ref _mainPaneButtonsActive, value);
        }

        public int ActiveMainPaneButtonIndex
        {
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
        
        public RoutingState Router { get; } = new();

        public MainWindowViewModel()
        {
            this.WhenAnyValue(vm => vm.IsMainPaneOpened).Subscribe(_ => _MainPaneOpenedOrClosed());
            Router.Navigate.Execute(new AccountViewModel(this));
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

        public static readonly MainPaneButtons WhenOpened = new()
        {
            Width = 180,
            Spacing = 8,
            Padding = Thickness.Parse("10 0 5 0"),
            HorizontalContentAlignment = HorizontalAlignment.Left,
            FontFamily = FontFamily.Default,
            FontSize = 20,
            Content = new[] { "Аккаунт", "Хранилище", "Генератор", "Настройки" },
        };
        
        public static readonly MainPaneButtons WhenClosed = new()
        {
            Width = 40,
            Spacing = 8,
            Padding = Thickness.Parse("0 0 0 0"),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            FontFamily = "Segoe MDL2 Assets",
            FontSize = 24,
            Content = new[] { "\uE77b", "\uE8F1", "\uEA80", "\uE713" },
        };
    }
}