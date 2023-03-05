using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.Base;

public abstract class PageViewModel : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment { get; }
        
    public IScreen HostScreen { get; }

    public virtual ContentControl[] RightBarButtons { get; } = Array.Empty<ContentControl>();

    protected PageViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;
        UrlPathSegment = GetType().Name;
    }
        
    protected void TryNavigateTo<TViewModel>(params object?[]? args) 
        where TViewModel : PageViewModel
    {
        args ??= Array.Empty<object>();
        var vm = (TViewModel)Activator.CreateInstance(typeof(TViewModel), new object[] {HostScreen}.Concat(args).ToArray())!;
        vm.TryNavigate();
    }
        
    protected void TryNavigateTo(Type viewModelType)
    {
        var vm = (PageViewModel)Activator.CreateInstance(viewModelType, HostScreen)!;
        vm.TryNavigate();
    }

    /// <summary>
    /// Refresh page.
    /// </summary>
    /// <remarks>Invokes with preloader.</remarks>
    public abstract Task RefreshAsync();

    /// <summary>
    /// Allow page closing.
    /// </summary>
    protected virtual Task<bool> OnCloseAsync() => Task.FromResult(true);

    public virtual void TryNavigate()
    {
        var navigateCommand = ReactiveCommand.CreateFromTask(async (Task<bool> allow) =>
        {
            if (!await allow) return;
            await HostScreen.Router.Navigate.Execute(this);
        });
            
        HostScreen.Router.CurrentViewModel.FirstOrDefaultAsync()
            .Select(async vm => vm is not PageViewModel page || await page.OnCloseAsync())
            .InvokeCommand(navigateCommand);
    }
}