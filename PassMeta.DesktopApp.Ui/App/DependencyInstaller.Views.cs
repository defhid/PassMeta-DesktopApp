using System.Runtime.CompilerServices;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.Account;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.JournalPage;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.LogsPage;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage;
using PassMeta.DesktopApp.Ui.Views.Pages;
using PassMeta.DesktopApp.Ui.Views.Pages.Account;
using PassMeta.DesktopApp.Ui.Views.Pages.Storage;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.App;

public static partial class DependencyInstaller
{
    /// <summary>
    /// Register page views for resolving by navigation.
    /// </summary>
    public static void RegisterPageViews()
    {
        RegisterView<AccountView, AccountPageModel>();
        RegisterView<AuthView, AuthPageModel>();
        RegisterView<PwdStorageView, PwdStoragePageModel>();
        RegisterView<GeneratorView, GeneratorPageModel>();
        RegisterView<JournalView, JournalPageModel>();
        RegisterView<LogsView, LogsPageModel>();
        RegisterView<SettingsView, SettingsPageModel>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RegisterView<TView, TViewModel>()
        where TView : IViewFor<TViewModel>, new()
        where TViewModel : class 
        => Locator.CurrentMutable.Register(() => new TView(), typeof(IViewFor<TViewModel>));
}