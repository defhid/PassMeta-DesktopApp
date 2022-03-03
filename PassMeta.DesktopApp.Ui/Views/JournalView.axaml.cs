namespace PassMeta.DesktopApp.Ui.Views
{
    using DesktopApp.Ui.Views.Base;
    
    using Avalonia.Markup.Xaml;
    using ViewModels.Logs;

    public class JournalView : ViewPage<LogsViewModel>
    {
        public JournalView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}