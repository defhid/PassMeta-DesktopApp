using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils;
using PassMeta.DesktopApp.Common.Conventions;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Dto.Internal;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Utils.PassFileContext;

/// <inheritdoc />
public class PassFileContext<TPassFile, TContent> : IPassFileContext<TPassFile>
    where TPassFile : PassFile<TContent>
    where TContent : class, new()
{
    private readonly BehaviorSubject<bool> _anyChangedSource = new(false);
    private readonly IPassFileLocalStorage _pfLocalStorage;
    private readonly IPassFileCryptoService _pfCryptoService;
    private readonly ICounter _counter;
    private readonly IMapper _mapper;
    private readonly IUserContext _userContext;
    private readonly IDialogService _dialogService;
    private readonly ILogService _logger;

    private Dictionary<long, PassFileState> _states = new();

    /// <summary></summary>
    public PassFileContext(
        IPassFileLocalStorage pfLocalStorage,
        IPassFileCryptoService pfCryptoService,
        ICounter counter,
        IMapper mapper,
        IUserContext userContext,
        IDialogService dialogService,
        ILogService logger)
    {
        Debug.Assert(userContext.UserId is not null);

        _pfLocalStorage = pfLocalStorage;
        _pfCryptoService = pfCryptoService;
        _counter = counter;
        _mapper = mapper;
        _userContext = userContext;
        _dialogService = dialogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public PassFileType PassFileType { get; } = PassFileConvention.GetPassFileType<TPassFile>();

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
            _states[dto.Id] = new PassFileState(
                _mapper.Map<PassFileLocalDto, TPassFile>(dto),
                _mapper.Map<PassFileLocalDto, TPassFile>(dto));
        }

        _anyChangedSource.OnNext(false);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<IResult> LoadContentAsync(TPassFile passFile, CancellationToken cancellationToken = default)
    {
        if (!FindStateWhereCurrentIs(passFile, out var state))
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
            Version = 1
        };

        var state = new PassFileState(null, _mapper.Map<PassFileLocalDto, TPassFile>(dto));

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
            if (!FindStateWhereCurrentIs(replacePassFile, out var state))
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
        if (!FindStateWhereCurrentIs(passFile, out _))
        {
            return UnexpectedError("Not from actual state");
        }

        if (fromOrigin)
        {
            passFile.OriginChangeStamps = new PassFileChangeStamps
            {
                InfoChangedOn = passFile.InfoChangedOn,
                VersionChangedOn = passFile.OriginChangeStamps?.VersionChangedOn ?? passFile.VersionChangedOn,
                Version = passFile.OriginChangeStamps?.Version ?? passFile.Version,
            };
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
        if (!FindStateWhereCurrentIs(passFile, out var state))
        {
            return UnexpectedError("Not from actual state");
        }

        if (fromOrigin)
        {
            passFile.OriginChangeStamps = new PassFileChangeStamps
            {
                InfoChangedOn = passFile.OriginChangeStamps?.InfoChangedOn ?? passFile.InfoChangedOn,
                VersionChangedOn = passFile.VersionChangedOn,
                Version = passFile.Version,
            };
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
    public IResult Delete(TPassFile passFile, bool fromOrigin)
    {
        if (!FindStateWhereCurrentIs(passFile, out var state))
        {
            return UnexpectedError("Not from actual state");
        }

        if (state.Source is null)
        {
            _states.Remove(passFile.Id);
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
    public IResult Restore(TPassFile passFile)
    {
        if (!FindStateWhereCurrentIs(passFile, out _))
        {
            return UnexpectedError("Not from actual state");
        }

        passFile.DeletedOn = null;

        if (passFile.Content.Any)
        {
            return UpdateContent(passFile, false);
        }

        _anyChangedSource.OnNext(DetectChanges());

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<IResult> CommitAsync()
    {
        var toSaveInfo = new List<TPassFile>();
        var toSaveContent = new List<TPassFile>();
        var toDeleteContent = new List<(TPassFile passFile, int version)>();
        var warnings = new List<string>();

        foreach (var state in _states.Values)
        {
            if (state.Current is null)
            {
                continue;
            }

            if (state.Current.VersionChangedOn != state.Source?.VersionChangedOn)
            {
                if (state.Current.Content.Encrypted is null)
                {
                    var res = _pfCryptoService.Encrypt(state.Current);
                    if (res.Bad)
                    {
                        return UnexpectedError(res.Message!);
                    }
                }

                toSaveContent.Add(state.Current);
            }

            toSaveInfo.Add(state.Current);
        }

        foreach (var passFile in toSaveContent)
        {
            var res = await _pfLocalStorage.GetVersionsAsync(passFile.Id, _userContext, CancellationToken.None);
            if (res.Bad)
            {
                warnings.Add($"{passFile.GetIdentityString()}: {res.Message}");
                continue;
            }

            toDeleteContent.AddRange(res.Data!
                .OrderByDescending(x => x)
                .Skip(AppConfig.KeepPassFileVersions - 1)
                .Select(x => (passFile, x)));
        }

        var result = await CommitInternalAsync(toSaveInfo, toSaveContent, toDeleteContent, warnings);

        if (warnings.Any())
        {
            foreach (var warn in warnings)
            {
                UnexpectedError(warn, show: false);
            }

            if (result.Ok)
            {
                _dialogService.ShowError(Resources.PASSCONTEXT__COMMIT_WARNING);
            }
        }

        return result;
    }

    private async Task<IResult> CommitInternalAsync(
        ICollection<TPassFile> toSaveInfo,
        IEnumerable<TPassFile> toSaveContent,
        IEnumerable<(TPassFile passFile, int version)> toDeleteContent,
        ICollection<string> warnings)
    {
        foreach (var passFile in toSaveContent)
        {
            var res = await _pfLocalStorage.SaveEncryptedContentAsync(
                passFile.Type, passFile.Id, passFile.Version,
                passFile.Content.Encrypted!, _userContext, CancellationToken.None);

            if (res.Bad)
            {
                return UnexpectedError(res.Message!);
            }
        }

        foreach (var (passFile, version) in toDeleteContent)
        {
            var res = await _pfLocalStorage.DeleteEncryptedContentAsync(
                passFile.Id, version, _userContext, CancellationToken.None);

            if (res.Bad)
            {
                warnings.Add($"{passFile.GetIdentityString()}: {res.Message}");
            }
        }

        var dtoList = toSaveInfo.Select(_mapper.Map<PassFile, PassFileLocalDto>);

        var result = await _pfLocalStorage.SaveListAsync(dtoList, _userContext, CancellationToken.None);
        if (result.Bad)
        {
            return UnexpectedError(result.Message!);
        }

        var states = new Dictionary<long, PassFileState>(toSaveInfo.Count);
        foreach (var source in toSaveInfo)
        {
            _states[source.Id] = new PassFileState(source, _mapper.Map<TPassFile, TPassFile>(source));
        }

        _states = states;
        _anyChangedSource.OnNext(false);

        return Result.Success();
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
                state.Current = _mapper.Map<TPassFile, TPassFile>(state.Source);
            }
            else
            {
                var clone = _mapper.Map<TPassFile, TPassFile>(state.Source);

                if (state.Source.Content.Encrypted is not null &&
                    state.Current.Content.PassPhrase is not null)
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

    private bool FindStateWhereCurrentIs(TPassFile passFile, out PassFileState state)
        => _states.TryGetValue(passFile.Id, out state!) &&
           ReferenceEquals(state.Current, passFile);

    private bool DetectChanges() => _states.Values.Any(x =>
        x.Source is null ||
        x.Current is null ||
        x.Source.InfoChangedOn != x.Current.InfoChangedOn ||
        x.Source.VersionChangedOn != x.Current.VersionChangedOn ||
        x.Source.DeletedOn != x.Current.DeletedOn);

    private IResult UnexpectedError(string log, bool show = true)
    {
        log = GetType().Name + ": " + log;
        _logger.Error(log);

        if (show)
        {
            _dialogService.ShowError(Resources.PASSCONTEXT_ERR, more: log);
        }

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