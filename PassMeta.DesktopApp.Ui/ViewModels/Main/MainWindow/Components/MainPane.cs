namespace PassMeta.DesktopApp.Ui.ViewModels.Main.MainWindow.Components
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using Avalonia;
    using Avalonia.Layout;
    using Avalonia.Media;
    using Common;
    using Constants;
    using ReactiveUI;

    public class MainPane : ReactiveObject
    {
        private bool _isOpened;
        public bool IsOpened
        {
            get => _isOpened;
            set => this.RaiseAndSetIfChanged(ref _isOpened, value);
        }

        private readonly ObservableAsPropertyHelper<int> _btnWidth;
        public int BtnWidth => _btnWidth.Value;

        private readonly ObservableAsPropertyHelper<Thickness> _btnPadding;
        public Thickness BtnPadding => _btnPadding.Value;
        
        private readonly ObservableAsPropertyHelper<HorizontalAlignment> _btnHorizontalContentAlignment;
        public HorizontalAlignment BtnHorizontalContentAlignment => _btnHorizontalContentAlignment.Value;
        
        private readonly ObservableAsPropertyHelper<FontFamily> _btnFontFamily;
        public FontFamily BtnFontFamily => _btnFontFamily.Value;
        
        private readonly ObservableAsPropertyHelper<int> _btnFontSize;
        public int BtnFontSize => _btnFontSize.Value;
        
        public ButtonCollection Buttons { get; }

        public MainPane()
        {
            var modeChanged = this.WhenAnyValue(pane => pane.IsOpened).Select(isOpened => !isOpened);

            _btnWidth = modeChanged.Select(isShort => isShort 
                ? 40 
                : 180).ToProperty(this, nameof(BtnWidth));
            
            _btnPadding = modeChanged.Select(isShort => isShort 
                ? Thickness.Parse("0 0 0 0") 
                : Thickness.Parse("10 0 5 4")).ToProperty(this, nameof(BtnPadding));
            
            _btnHorizontalContentAlignment = modeChanged.Select(isShort => isShort 
                ? HorizontalAlignment.Center 
                : HorizontalAlignment.Left).ToProperty(this, nameof(BtnHorizontalContentAlignment));
            
            _btnFontFamily = modeChanged.Select(isShort => isShort 
                ? FontFamilies.SegoeMdl2 
                : FontFamilies.Default).ToProperty(this, nameof(BtnFontFamily));
            
            _btnFontSize = modeChanged.Select(isShort => isShort
                ? 24 
                : 20).ToProperty(this, nameof(BtnFontSize));

            Buttons = new ButtonCollection(modeChanged);
        }

        public class ButtonCollection
        {
            private readonly MainPaneBtn[] _all;
            
            public MainPaneBtn? CurrentActive 
            { 
                get => _all.FirstOrDefault(btn => btn.IsActive);
                set
                {
                    foreach (var btn in _all)
                    {
                        btn.IsActive = ReferenceEquals(btn, value);
                    }
                }
            }

            public MainPaneBtn Account { get; }

            public MainPaneBtn Storage { get; }
            
            public MainPaneBtn Generator { get; }
            
            public MainPaneBtn Settings { get; }
            
            public ButtonCollection(IObservable<bool> modeChanged)
            {
                Account = new MainPaneBtn(Resources.MAIN_MENU__ACCOUNT_BTN, "\uE77b", modeChanged);
                Storage = new MainPaneBtn(Resources.MAIN_MENU__STORAGE_BTN, "\uE8F1", modeChanged);
                Generator = new MainPaneBtn(Resources.MAIN_MENU__GENERATOR_BTN, "\uEA80", modeChanged);
                Settings = new MainPaneBtn(Resources.MAIN_MENU__SETTINGS_BTN, "\uE713", modeChanged);
                
                _all = new[] { Account, Storage, Generator, Settings };
            }
        }
    }
}