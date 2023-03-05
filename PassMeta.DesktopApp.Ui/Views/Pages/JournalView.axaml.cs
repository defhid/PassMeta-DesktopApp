using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.JournalPage;

namespace PassMeta.DesktopApp.Ui.Views.Pages;

public class JournalView : ReactiveUserControl<JournalPageModel>
{
    public JournalView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}