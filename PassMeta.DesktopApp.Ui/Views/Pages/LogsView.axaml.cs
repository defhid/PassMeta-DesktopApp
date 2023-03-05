using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.LogsPage;

namespace PassMeta.DesktopApp.Ui.Views.Pages;

public class LogsView : ReactiveUserControl<LogsPageModel>
{
    public LogsView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}