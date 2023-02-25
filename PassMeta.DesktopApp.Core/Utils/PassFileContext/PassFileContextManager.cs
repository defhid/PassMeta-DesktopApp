using System;
using System.Collections.Generic;
using AutoMapper;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Core.Extensions;
using Splat;

namespace PassMeta.DesktopApp.Core.Utils.PassFileContext;

/// <inheritdoc />
public class PassFileContextManager : IPassFileContextManager
{
    private readonly IReadonlyDependencyResolver _dependencyResolver;
    private IPassFileContext[] _contexts = Array.Empty<IPassFileContext>();

    /// <summary></summary>
    public PassFileContextManager(IReadonlyDependencyResolver dependencyResolver)
    {
        _dependencyResolver = dependencyResolver;
    }

    /// <inheritdoc />
    public IEnumerable<IPassFileContext> Contexts => _contexts;

    /// <inheritdoc />
    public void Reload()
    {
        foreach (var context in _contexts)
        {
            context.Dispose();
        }

        _contexts = new[]
        {
            CreateContext<PwdPassFile, List<PwdSection>>(),
            CreateContext<TxtPassFile, List<TxtSection>>(),
        };
    }

    private IPassFileContext CreateContext<TPassFile, TContent>()
        where TPassFile : PassFile<TContent>
        where TContent : class, new() 
        => new PassFileContext<TPassFile, TContent>(
            _dependencyResolver.Resolve<IPassFileLocalStorage>(),
            _dependencyResolver.Resolve<IPassFileCryptoService>(),
            _dependencyResolver.Resolve<ICounter>(),
            _dependencyResolver.Resolve<IMapper>(),
            _dependencyResolver.Resolve<IUserContext>(),
            _dependencyResolver.Resolve<IDialogService>(),
            _dependencyResolver.Resolve<ILogsWriter>());
}