using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.Logs;

namespace PassMeta.DesktopApp.Ui.Views;

public class LogsView : ReactiveUserControl<LogsModel>
{
    public LogsView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}