using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Common.Models.Entities.PassFileMerge;

namespace PassMeta.DesktopApp.Core.Services.PassFileServices;

/// <inheritdoc />
public class PwdPassFileMergePreparingService : IPwdPassFileMergePreparingService
{
    private readonly IPassFileContextProvider _passFileContextProvider;
    private readonly IPassFileRemoteService _remoteService;
    private readonly IPassFileCryptoService _passFileCryptoService;
    private readonly IDialogService _dialogService;

    /// <summary></summary>
    public PwdPassFileMergePreparingService(
        IPassFileContextProvider passFileContextProvider,
        IPassFileRemoteService remoteService,
        IPassFileCryptoService passFileCryptoService,
        IDialogService dialogService)
    {
        _passFileContextProvider = passFileContextProvider;
        _remoteService = remoteService;
        _passFileCryptoService = passFileCryptoService;
        _dialogService = dialogService;
    }

    /// <inheritdoc />
    public async Task<IResult<PwdPassFileMerge>> LoadAndPrepareMergeAsync(PwdPassFile localPassFile)
    {
        if (localPassFile.Content.Decrypted is null)
        {
            if (localPassFile.Content.Encrypted is null)
            {
                var result = await _passFileContextProvider.For<PwdPassFile>().LoadEncryptedContentAsync(localPassFile);
                if (result.Bad)
                {
                    return Result.Failure<PwdPassFileMerge>();
                }
            }

            if (!await _DecryptAsync(localPassFile, 
                    Resources.PASSMERGE__ASK_PASSPHRASE, Resources.PASSMERGE__ASK_PASSPHRASE_AGAIN))
            {
                return Result.Failure<PwdPassFileMerge>();
            }
        }

        var fetchResult = await _remoteService.GetInfoAsync(localPassFile);
        if (fetchResult.Bad)
            return Result.Failure<PwdPassFileMerge>();

        var remotePassFile = fetchResult.Data!;
        if (!await _DecryptAsync(remotePassFile, 
                Resources.PASSMERGE__ASK_PASSPHRASE_REMOTE, Resources.PASSMERGE__ASK_PASSPHRASE_REMOTE_AGAIN, localPassFile))
        {
            return Result.Failure<PwdPassFileMerge>();
        }

        var merge = _PrepareMerge(localPassFile, remotePassFile);

        return Result.Success(merge);
    }

    private static PwdPassFileMerge _PrepareMerge(PwdPassFile localPassFile, PwdPassFile remotePassFile)
    {
        var merge = new PwdPassFileMerge(localPassFile, remotePassFile);

        var localList = localPassFile.Content.Decrypted!.Select(section => section.Copy()).ToList();
        var remoteList = remotePassFile.Content.Decrypted!.Select(section => section.Copy()).ToList();

        FillMerge(localList, remoteList, merge, false);
        FillMerge(remoteList, localList, merge, true);

        return merge;
    }
    
    private static void FillMerge(
        IList<PwdSection> localSections,
        ICollection<PwdSection> remoteSections,
        PwdPassFileMerge merge,
        bool reverse)
    {
        for (var i = localSections.Count - 1; i >= 0; --i)
        {
            var local = localSections[i];
            var remote = remoteSections.FirstOrDefault(section => section.Id == local.Id);

            if (remote is null)
            {
                merge.Conflicts.Add(new PwdPassFileMerge.Conflict(reverse ? null : local, reverse ? local : null));
            }
            else if (remote.DiffersFrom(local))
            {
                remoteSections.Remove(remote);
                merge.Conflicts.Add(new PwdPassFileMerge.Conflict(reverse ? remote : local, reverse ? local : remote));
            }
            else if (remote.Items.Count != local.Items.Count ||
                     remote.Items.Exists(it1 =>
                         local.Items.All(it2 => it2.DiffersFrom(it1))))
            {
                remoteSections.Remove(remote);
                merge.Conflicts.Add(new PwdPassFileMerge.Conflict(reverse ? remote : local, reverse ? local : remote));
            }
            else
            {
                remoteSections.Remove(remote);
                merge.Result.Add(local);
            }
        }

        localSections.Clear();
    }
        
    private async Task<bool> _DecryptAsync(PwdPassFile passFile, string askPhraseFirst, string askPhraseAgain, PwdPassFile? localPassFile = null)
    {
        if (passFile.Content.PassPhrase is not null)
        {
            if (_passFileCryptoService.Decrypt(passFile, silent: true).Ok)
            {
                return true;
            }
        }

        if (localPassFile is not null)
        {
            if (_passFileCryptoService.Decrypt(passFile, localPassFile.Content.PassPhrase, silent: true).Ok)
            {
                return true;
            }
        }

        var passPhrase = await _dialogService.AskPasswordAsync(askPhraseFirst);

        while (passPhrase.Ok && (
                   passPhrase.Data == string.Empty ||
                   _passFileCryptoService.Decrypt(passFile, passPhrase.Data!).Bad))
        {
            passPhrase = await _dialogService.AskPasswordAsync(askPhraseAgain);
        }

        return passFile.Content.Decrypted is not null;
    }
}