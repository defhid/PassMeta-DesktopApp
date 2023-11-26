using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views.Windows.MainWin;

public partial class MainWindow : ReactiveWindow<MainWinModel>
{
    private bool _closingConfirmed;

    public MainWindow()
    {
        Opened += OnOpened;
        Closing += OnClosing;

        InitializeComponent();

#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        var navigate = Locator.Current.Resolve<IUserContextProvider>().Current.UserId is null
            ? ViewModel!.MainPane.Account.Command
            : ViewModel!.MainPane.Storage.Command;

        navigate.Execute(null);
    }

    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        var pfcManager = Locator.Current.Resolve<IPassFileContextManager>();

        if (_closingConfirmed || pfcManager.Contexts.All(x => !x.AnyChanged))
        {
            return;
        }

        e.Cancel = true;
        var dialogService = Locator.Current.Resolve<IDialogService>();

        var confirm = await dialogService.ConfirmAsync(Common.Resources.APP__CONFIRM_ROLLBACK_ON_QUIT);
        if (!confirm.Ok) return;

        _closingConfirmed = true;
        Close();
    }
}