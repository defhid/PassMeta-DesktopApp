using PassMeta.DesktopApp.Common.Abstractions.AppConfig;
using PassMeta.DesktopApp.Core.Extensions;
using Splat;

namespace PassMeta.DesktopApp.Ui.ViewModels.Main.MainWindow.Components;

using System;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using Common;
using Constants;
using ReactiveUI;

public sealed class MainPane : ReactiveObject, IDisposable
{
    private bool _isOpened;
    public bool IsOpened
    {
        get => _isOpened;
        set => this.RaiseAndSetIfChanged(ref _isOpened, value);
    }

    public IObservable<int> BtnWidth { get; }

    public IObservable<Thickness> BtnPadding { get; }
        
    public IObservable<HorizontalAlignment> BtnHorizontalContentAlignment { get; }
        
    public IObservable<FontFamily> BtnFontFamily { get; }
        
    public IObservable<int> BtnFontSize { get; }
        
    public ButtonCollection Buttons { get; }

    public MainPane()
    {
        var modeChanged = this.WhenAnyValue(pane => pane.IsOpened).Select(isOpened => !isOpened);

        BtnWidth = modeChanged.Select(isShort => isShort 
            ? 40 
            : 180);
            
        BtnPadding = modeChanged.Select(isShort => isShort 
            ? Thickness.Parse("0 0 0 0") 
            : Thickness.Parse("10 0 5 4"));
            
        BtnHorizontalContentAlignment = modeChanged.Select(isShort => isShort 
            ? HorizontalAlignment.Center 
            : HorizontalAlignment.Left);
            
        BtnFontFamily = modeChanged.Select(isShort => isShort 
            ? FontFamilies.SegoeMdl2 
            : FontFamilies.Default);
            
        BtnFontSize = modeChanged.Select(isShort => isShort
            ? 28 
            : 20);

        Buttons = new ButtonCollection(modeChanged);
    }

    public void Dispose()
    {
        Buttons.Dispose();
    }

    public sealed class ButtonCollection : IDisposable
    {
        private readonly MainPaneBtn[] _all;
        private readonly IDisposable[] _disposables;

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
            
        public MainPaneBtn Journal { get; }
            
        public MainPaneBtn Logs { get; }
            
        public MainPaneBtn Settings { get; }
            
        public ButtonCollection(IObservable<bool> modeChanged)
        {
            Account = new MainPaneBtn(Resources.APP__MENU_BTN__ACCOUNT, "\uE77b", modeChanged);
            Storage = new MainPaneBtn(Resources.APP__MENU_BTN__STORAGE, "\uE8F1", modeChanged);
            Generator = new MainPaneBtn(Resources.APP__MENU_BTN__GENERATOR, "\uEA80", modeChanged);
            Journal = new MainPaneBtn(Resources.APP__MENU_BTN__JOURNAL, "\uE823", modeChanged);
            Logs = new MainPaneBtn(Resources.APP__MENU_BTN__LOGS, "\uE9D9", modeChanged) { IsVisible = false };
            Settings = new MainPaneBtn(Resources.APP__MENU_BTN__SETTINGS, "\uE713", modeChanged);
                
            _all = new[] { Account, Storage, Generator, Journal, Logs, Settings };

            _disposables = new[]
            {
                Locator.Current.Resolve<IAppConfigProvider>().CurrentObservable.Subscribe(x =>
                {
                    Logs.IsVisible = x.DevMode;
                })
            };
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}