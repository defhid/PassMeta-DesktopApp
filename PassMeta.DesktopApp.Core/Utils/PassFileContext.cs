using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Dto.Internal;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Utils;

/// <inheritdoc />
public abstract class PassFileContext<TPassFile, TContent> : IPassFileContext<TPassFile>
    where TPassFile : PassFile<TContent>
    where TContent : class, new()
{
    private readonly BehaviorSubject<bool> _anyChangedSource = new(false);
    private readonly IPassFileLocalStorage _pfLocalStorage;
    private readonly IPassFileCryptoService _pfCryptoService;
    private readonly ICounter _counter;
    private readonly IUserContext _userContext;
    private readonly IDialogService _dialogService;
    private readonly ILogService _logger;

    private Dictionary<long, PassFileState> _states = new();

    /// <summary></summary>
    protected PassFileContext(
        IPassFileLocalStorage pfLocalStorage,
        IPassFileCryptoService pfCryptoService,
        ICounter counter,
        IUserContext userContext,
        IDialogService dialogService,
        ILogService logger)
    {
        Debug.Assert(userContext.UserId is not null);

        _pfLocalStorage = pfLocalStorage;
        _pfCryptoService = pfCryptoService;
        _counter = counter;
        _userContext = userContext;
        _dialogService = dialogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public abstract PassFileType PassFileType { get; }

    /// <inheritdoc />
    public bool AnyChanged => _anyChangedSource.Value;

    /// <inheritdoc />
    public IObservable<bool> AnyChangedSource => _anyChangedSource;

    /// <inheritdoc />
    public IEnumerable<TPassFile> CurrentList => _states.Values
        .Select(x => x.Current)
        .Where(x => x is not null)!;

    /// <inheritdoc />
    public async Task<IResult> LoadListAsync(CancellationToken cancellationToken = default)
    {
        var result = await _pfLocalStorage.LoadListAsync(_userContext, cancellationToken);
        if (result.Bad)
        {
            return UnexpectedError(result.Message!);
        }

        _states = new Dictionary<long, PassFileState>();

        foreach (var dto in result.Data!.Where(x => x.Type == PassFileType))
        {
            _states[dto.Id] = new PassFileState(Map(dto), Map(dto));
        }

        _anyChangedSource.OnNext(false);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<IResult> LoadContentAsync(TPassFile passFile, CancellationToken cancellationToken = default)
    {
        if (!FindState(passFile, out var state))
        {
            return UnexpectedError("Not from actual state");
        }

        var result = await _pfLocalStorage.LoadEncryptedContentAsync(
            passFile.Type, passFile.Id, passFile.Version, _userContext, cancellationToken);

        if (result.Bad)
        {
            return UnexpectedError(result.Message!);
        }

        state.Current!.Content = new PassFileContent<TContent>(result.Data!);
        state.Source!.Content = new PassFileContent<TContent>(result.Data!);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<TPassFile> CreateAsync(CancellationToken cancellationToken = default)
    {
        const string idSequenceName = "pfid";

        var dto = new PassFileLocalDto
        {
            Id = -await _counter.GetNextValueAsync(idSequenceName, 0L, cancellationToken),
            UserId = _userContext.UserId!.Value,
            Type = PassFileType,
            Name = Resources.PASSCONTEXT__DEFAULT_NEW_PASSFILE_NAME,
            CreatedOn = DateTime.UtcNow,
            InfoChangedOn = DateTime.UtcNow,
            VersionChangedOn = DateTime.UtcNow,
            Version = 1,
        };

        var state = new PassFileState(null, Map(dto));

        _states[state.Current!.Id] = state;
        _anyChangedSource.OnNext(true);

        return state.Current;
    }

    /// <inheritdoc />
    public IResult Add(TPassFile originPassFile, TPassFile? replacePassFile)
    {
        Debug.Assert(originPassFile.Type == PassFileType);

        if (replacePassFile is not null)
        {
            if (!FindState(replacePassFile, out var state))
            {
                return UnexpectedError("Not from actual state");
            }

            state.Current = originPassFile;
        }
        else
        {
            _states[originPassFile.Id] = new PassFileState(null, originPassFile);
        }
        
        _anyChangedSource.OnNext(true);
        return Result.Success();
    }

    /// <inheritdoc />
    public IResult UpdateInfo(TPassFile passFile, bool fromOrigin)
    {
        if (!FindState(passFile, out _))
        {
            return UnexpectedError("Not from actual state");
        }

        if (fromOrigin)
        {
            passFile.OriginChangeStamps = Map(new PassFileLocalDto
            {
                InfoChangedOn = passFile.InfoChangedOn,
                VersionChangedOn = passFile.OriginChangeStamps?.VersionChangedOn ?? passFile.VersionChangedOn,
                Version = passFile.OriginChangeStamps?.Version ?? passFile.Version,
            });
        }
        else
        {
            passFile.InfoChangedOn = DateTime.UtcNow;
        }

        _anyChangedSource.OnNext(true);

        return Result.Success();
    }

    /// <inheritdoc />
    public IResult UpdateContent(TPassFile passFile, bool fromOrigin)
    {
        if (!FindState(passFile, out var state))
        {
            return UnexpectedError("Not from actual state");
        }
        
        if (fromOrigin)
        {
            passFile.OriginChangeStamps = Map(new PassFileLocalDto
            {
                InfoChangedOn = passFile.OriginChangeStamps?.InfoChangedOn ?? passFile.InfoChangedOn,
                VersionChangedOn = passFile.VersionChangedOn,
                Version = passFile.Version,
            });
        }
        else
        {
            passFile.VersionChangedOn = DateTime.UtcNow;

            if (state.Source?.Version == state.Current!.Version)
            {
                ++passFile.Version;
            }
        }

        _anyChangedSource.OnNext(true);

        return Result.Success();
    }

    /// <inheritdoc />
    public IResult Delete(int passFileId, bool fromOrigin)
    {
        if (!FindState(passFileId, out var state))
        {
            return UnexpectedError("Not from actual state");
        }

        if (state.Source is null)
        {
            _states.Remove(passFileId);
        }
        else if (state.Current!.IsLocalCreated() || fromOrigin)
        {
            state.Current = null;
        }
        else
        {
            state.Current!.DeletedOn = DateTime.UtcNow;
        }

        _anyChangedSource.OnNext(DetectChanges());

        return Result.Success();
    }
    
    /// <inheritdoc />
    public Task<IResult> CommitAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void Rollback()
    {
        foreach (var (passFileId, state) in _states.ToList())
        {
            if (state.Source is null)
            {
                _states.Remove(passFileId);
            }
            else if (state.Current is null)
            {
                state.Current = Map(Map(state.Source));
            }
            else
            {
                var clone = Map(Map(state.Source));

                if (state.Source.Content.Encrypted is not null && state.Current.Content.PassPhrase is not null)
                {
                    clone.Content = 
                        new PassFileContent<TContent>(state.Source.Content.Encrypted, state.Current.Content.PassPhrase);
                }

                state.Current = clone;
            }
        }

        _anyChangedSource.OnNext(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _states.Clear();
        _anyChangedSource.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Parse passfile dto to entity.
    /// </summary>
    protected abstract TPassFile Map(PassFileLocalDto dto);

    /// <summary>
    /// Parse passfile dto to entity.
    /// </summary>
    private static PassFileLocalDto Map(TPassFile entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        Type = entity.Type,
        Name = entity.Name,
        Color = entity.Color,
        Version = entity.Version,
        CreatedOn = entity.CreatedOn,
        DeletedOn = entity.DeletedOn,
        InfoChangedOn = entity.InfoChangedOn,
        VersionChangedOn = entity.VersionChangedOn,
        Origin = entity.OriginChangeStamps is null 
            ? null 
            : new PassFileLocalDto
            {
                InfoChangedOn = entity.InfoChangedOn,
                VersionChangedOn = entity.VersionChangedOn,
                Version = entity.Version
            }
    };

    private bool FindState(TPassFile passFile, out PassFileState state)
        => _states.TryGetValue(passFile.Id, out state!) &&
           ReferenceEquals(state.Current, passFile);
    
    private bool FindState(long passFileId, out PassFileState state)
        => _states.TryGetValue(passFileId, out state!) &&
           state.Current is not null;

    private bool DetectChanges() => _states.Values.Any(x =>
        x.Source is null ||
        x.Current is null ||
        x.Source.InfoChangedOn != x.Current.InfoChangedOn ||
        x.Source.VersionChangedOn != x.Current.VersionChangedOn ||
        x.Source.DeletedOn != x.Current.DeletedOn);

    private IResult UnexpectedError(string log, Exception? ex = null)
    {
        log = nameof(PassFileManager) + ": " + log;

        if (ex is null) _logger.Error(log);
        else _logger.Error(ex, log);

        _dialogService.ShowError(Resources.PASSCONTEXT_ERR, more: ex?.Message);
        return Result.Failure();
    }

    private class PassFileState
    {
        public readonly TPassFile? Source;
        public TPassFile? Current;

        public PassFileState(TPassFile? source, TPassFile? current)
        {
            Source = source;
            Current = current;
        }
    }
}