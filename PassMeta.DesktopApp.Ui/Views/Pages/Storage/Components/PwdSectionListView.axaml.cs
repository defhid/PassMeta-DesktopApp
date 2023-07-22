using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace PassMeta.DesktopApp.Ui.Views.Pages.Storage.Components;

public partial class PwdSectionListView : UserControl
{
    public PwdSectionListView()
    {
        InitializeComponent();
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