using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.JournalPage;

namespace PassMeta.DesktopApp.Ui.Views.Pages;

public partial class JournalView : ReactiveUserControl<JournalPageModel>
{
    public JournalView()
    {
        InitializeComponent();
    }
}