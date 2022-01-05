﻿namespace PassMeta.DesktopApp.Ui.ViewModels.Main.MainWindow
{
    using Avalonia.Controls;
    using Components;
    using Interfaces;
    using ReactiveUI;

    public class MainWindowViewModel : ReactiveObject, IScreen, IPreloaderSupport
    {
        public RoutingState Router { get; } = new();

        public MainPane MainPane { get; } = new();

        public AppMode Mode { get; } = new();

        private ContentControl[]? _rightBarButtons;
        public ContentControl[]? RightBarButtons
        {
            get => _rightBarButtons;
            set => this.RaiseAndSetIfChanged(ref _rightBarButtons, value);
        }
        
        private bool _preloaderEnabled;
        public bool PreloaderEnabled
        {
            get => _preloaderEnabled;
            set => this.RaiseAndSetIfChanged(ref _preloaderEnabled, value);
        }
    }
}