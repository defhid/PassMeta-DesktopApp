using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.Journal;

namespace PassMeta.DesktopApp.Ui.Views;

public class JournalView : ReactiveUserControl<JournalModel>
{
    public JournalView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}