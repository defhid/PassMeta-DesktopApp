using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Models;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Base;

/// <summary>
/// Base page ViewModel.
/// </summary>
public abstract class PageViewModel : ReactiveObject, IActivatableViewModel, IRoutableViewModel
{
    /// <summary></summary>
    protected PageViewModel(IScreen hostScreen)
    {
        Activator = new ViewModelActivator();
        UrlPathSegment = GetType().Name;
        HostScreen = hostScreen;
    }

    /// <inheritdoc />
    public ViewModelActivator Activator { get; }

    /// <inheritdoc />
    public string? UrlPathSegment { get; }
        
    /// <inheritdoc />
    public IScreen HostScreen { get; }

    /// <summary>
    /// Action buttons to display on the right bar.
    /// </summary>
    public virtual ContentControl[] RightBarButtons { get; } = Array.Empty<ContentControl>();

    /// <summary>
    /// Refresh page data.
    /// </summary>
    public abstract ValueTask RefreshAsync();

    /// <summary>
    /// Is it allowed to leave this ViewModel.
    /// </summary>
    protected virtual ValueTask<IResult> CanLeaveAsync() => ValueTask.FromResult<IResult>(Result.Success());

    /// <summary>
    /// Try to navigate to this ViewModel.
    /// </summary>
    public async ValueTask<bool> TryNavigateAsync()
    {
        var current = await HostScreen.Router.CurrentViewModel.FirstAsync();
        if (current is PageViewModel page)
        {
            var canLeaveResult = await page.CanLeaveAsync();
            if (canLeaveResult.Bad)
            {
                return false;
            }
        }

        await HostScreen.Router.Navigate.Execute(this);
        return true;
    }
}