using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.LogsPage;

namespace PassMeta.DesktopApp.Ui.Views.Pages;

public partial class LogsView : ReactiveUserControl<LogsPageModel>
{
    public LogsView()
    {
        InitializeComponent();
    }
}