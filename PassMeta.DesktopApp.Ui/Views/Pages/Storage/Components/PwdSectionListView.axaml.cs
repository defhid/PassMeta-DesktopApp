using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;

namespace PassMeta.DesktopApp.Ui.Views.Pages.Storage.Components;

public class PwdSectionListView : UserControl
{
    public PwdSectionListView()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Search_OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        // ViewModel!.ViewElements.SearchBox = (TextBox)sender!;
    }

    private void SectionListBox_OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        // ViewModel!.ViewElements.SectionListBox = (ListBox)sender!;
    }
}