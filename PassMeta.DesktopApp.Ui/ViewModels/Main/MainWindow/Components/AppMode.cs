namespace PassMeta.DesktopApp.Ui.ViewModels.Main.MainWindow.Components
{
    using Avalonia.Media;
    using Common;
    using Core.Utils;
    using ReactiveUI;

    public class AppMode : ReactiveObject
    {
        public string Text => PassMetaApi.Online ? Resources.APP__ONLINE_MODE : Resources.APP__OFFLINE_MODE;
        public IBrush Foreground => PassMetaApi.Online ? Brushes.Green : Brushes.SlateGray;
        
        public void OnChanged()
        {
            this.RaisePropertyChanged(nameof(Text));
            this.RaisePropertyChanged(nameof(Foreground));
        }
    }
}