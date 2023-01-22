using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.UserContext;
using PassMeta.DesktopApp.Core.Models;

namespace PassMeta.DesktopApp.Core.Utils;

/// <inheritdoc cref="IUserContextProvider" />
public sealed class UserContextProvider : IUserContextProvider, IDisposable
{
    private readonly BehaviorSubject<UserContextModel> _appContextSubject;
    private readonly IDisposable _disposable;

    /// <summary></summary>
    public UserContextProvider(IAppContextProvider appContextProvider)
    {
        _appContextSubject = new BehaviorSubject<UserContextModel>(new UserContextModel(null, null));

        _disposable = appContextProvider.CurrentObservable
            .Select(MakeUserContext)
            .Subscribe(userContext =>
            {
                if (userContext.UniqueId != _appContextSubject.Value.UniqueId)
                {
                    _appContextSubject.OnNext(userContext);
                }
            });

        _appContextSubject.OnNext(MakeUserContext(appContextProvider.Current));
    }

    /// <inheritdoc />
    public IUserContext Current => _appContextSubject.Value;

    /// <inheritdoc />
    public IObservable<IUserContext> CurrentObservable => _appContextSubject;

    /// <inheritdoc />
    public void Dispose()
    {
        _appContextSubject.Dispose();
        _disposable.Dispose();
    }

    private static UserContextModel MakeUserContext(IAppContext appContext) 
        => new(appContext.User?.Id, appContext.ServerId);
}