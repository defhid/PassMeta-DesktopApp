using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
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
            ViewModel!.Items.CollectionChanged += OnItemAdded;
            Disposable
                .Create(() => ViewModel!.Items.CollectionChanged -= OnItemAdded)
                .DisposeWith(disposables);

            ViewModel
                .WhenAnyValue(x => x.IsVisible)
                .Where(visible => visible)
                .Subscribe(_ => OnSectionShow())
                .DisposeWith(disposables);
        });
    }

    private void OnItemAdded(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (ViewModel!.IsVisible &&
            args.Action is NotifyCollectionChangedAction.Add)
        {
            this.FindControl<ScrollViewer>("RootScrollViewer")?.ScrollToEnd();
        }
    }

    private void OnSectionShow()
    {
        var sectionNameBox = this.FindControl<TextBox>("NameTextBox");
        if (sectionNameBox is null)
        {
            Debug.Assert(sectionNameBox is not null);
            return;
        }

        if (ViewModel!.IsNew)
        {
            sectionNameBox.SelectionStart = 0;
            sectionNameBox.SelectionEnd = ViewModel.Name?.Length ?? 0;
            sectionNameBox.Focus(NavigationMethod.Pointer);
        }
        else
        {
            sectionNameBox.SelectionStart = ViewModel.Name?.Length ?? 0;
            sectionNameBox.SelectionEnd = ViewModel.Name?.Length ?? 0;
        }
    }
}