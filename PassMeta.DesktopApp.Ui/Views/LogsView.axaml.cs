namespace PassMeta.DesktopApp.Ui.Views
{
    using DesktopApp.Ui.Views.Base;
    
    using Avalonia.Markup.Xaml;
    using ViewModels.Logs;

    public class LogsView : ViewPage<LogsViewModel>
    {
        public LogsView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}