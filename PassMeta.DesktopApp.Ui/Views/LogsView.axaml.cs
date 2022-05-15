namespace PassMeta.DesktopApp.Ui.Views
{
    using DesktopApp.Ui.Views.Base;
    
    using Avalonia.Markup.Xaml;
    using ViewModels.Logs;

    public class LogsView : PageView<LogsViewModel>
    {
        public LogsView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}