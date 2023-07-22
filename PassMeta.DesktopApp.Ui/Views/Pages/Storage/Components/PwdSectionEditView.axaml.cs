using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Views.Pages.Storage.Components;

public partial class PwdSectionEditView : ReactiveUserControl<PwdSectionEditModel>
{
    public PwdSectionEditView()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            ViewModel!.ScrollToBottom
                .RegisterHandler(_ => this.FindControl<ScrollViewer>("RootScrollViewer").ScrollToEnd())
                .DisposeWith(disposables);
        });
    }
}