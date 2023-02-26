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

namespace PassMeta.DesktopApp.Core.Utils.PassFileContext;

/// <inheritdoc />
public class PassFileContextManager : IPassFileContextManager
{
    private readonly IPassFileLocalStorage _pfLocalStorage;
    private readonly IPassFileCryptoService _pfCryptoService;
    private readonly ICounter _counter;
    private readonly IMapper _mapper;
    private readonly IDialogService _dialogService;
    private readonly ILogsWriter _logger;
    private IPassFileContext[] _contexts = Array.Empty<IPassFileContext>();

    /// <summary></summary>
    public PassFileContextManager(
        IPassFileLocalStorage pfLocalStorage,
        IPassFileCryptoService pfCryptoService,
        ICounter counter,
        IMapper mapper,
        IDialogService dialogService,
        ILogsWriter logger)
    {
        _pfLocalStorage = pfLocalStorage;
        _pfCryptoService = pfCryptoService;
        _counter = counter;
        _mapper = mapper;
        _dialogService = dialogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public IEnumerable<IPassFileContext> Contexts => _contexts;

    /// <inheritdoc />
    public void Reload(IUserContext userContext)
    {
        foreach (var context in _contexts)
        {
            context.Dispose();
        }

        _contexts = new[]
        {
            CreateContext<PwdPassFile, List<PwdSection>>(userContext),
            CreateContext<TxtPassFile, List<TxtSection>>(userContext),
        };
    }

    private IPassFileContext CreateContext<TPassFile, TContent>(IUserContext userContext)
        where TPassFile : PassFile<TContent>
        where TContent : class, new() 
        => new PassFileContext<TPassFile, TContent>(
            _pfLocalStorage, _pfCryptoService, _counter, _mapper, userContext, _dialogService, _logger);
}