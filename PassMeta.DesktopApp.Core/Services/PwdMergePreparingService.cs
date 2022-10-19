namespace PassMeta.DesktopApp.Core.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Common.Abstractions;
    using Common.Abstractions.Services;
    using Common.Abstractions.Services.PassFile;
    using Common.Models;
    using Common.Models.Dto;
    using Common.Models.Entities;
    using Common.Models.Entities.Extra;
    using Utils;
    using Utils.Extensions;

    /// <inheritdoc />
    public class PwdMergePreparingService : IPwdMergePreparingService
    {
        private readonly IPassFileRemoteService _remoteService = EnvironmentContainer.Resolve<IPassFileRemoteService>();
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();

        /// <inheritdoc />
        public async Task<IResult<PwdMerge>> LoadAndPrepareMergeAsync(PassFile localPassFile)
        {
            if (localPassFile.PwdData is null)
            {
                var result = await PassFileManager.GetEncryptedDataAsync(localPassFile.Type, localPassFile.Id);
                if (result.Bad)
                {
                    _dialogService.ShowFailure(result.Message!);
                }

                localPassFile.DataEncrypted = result.Data!;
                if (!await _DecryptAsync(localPassFile, 
                        Resources.PASSMERGE__ASK_PASSPHRASE, Resources.PASSMERGE__ASK_PASSPHRASE_AGAIN))
                {
                    return Result.Failure<PwdMerge>();
                }
            }

            var fetchResult = await _remoteService.GetAsync(localPassFile.Id);
            if (fetchResult.Bad)
                return Result.Failure<PwdMerge>();

            var remotePassFile = fetchResult.Data!;
            if (!await _DecryptAsync(remotePassFile, 
                    Resources.PASSMERGE__ASK_PASSPHRASE_REMOTE, Resources.PASSMERGE__ASK_PASSPHRASE_REMOTE_AGAIN, localPassFile))
            {
                return Result.Failure<PwdMerge>();
            }

            var merge = _PrepareMerge(localPassFile, remotePassFile);

            return Result.Success(merge);
        }

        private static PwdMerge _PrepareMerge(PassFile localPassFile, PassFile remotePassFile)
        {
            void FillMerge(
                IList<PwdSection> localSections,
                ICollection<PwdSection> remoteSections,
                PwdMerge merge,
                bool reverse)
            {
                for (var i = localSections.Count - 1; i >= 0; --i)
                {
                    var local = localSections[i];
                    var remote = remoteSections.FirstOrDefault(section => section.Id == local.Id);

                    if (remote is null)
                    {
                        merge.Conflicts.Add(new PwdMerge.Conflict(reverse ? null : local, reverse ? local : null));
                    }
                    else if (remote.Items.Count != local.Items.Count ||
                             remote.Items.Exists(it1 => 
                                 local.Items.All(it2 => it2.DiffersFrom(it1))))
                    {
                        remoteSections.Remove(remote);
                        merge.Conflicts.Add(new PwdMerge.Conflict(reverse ? remote : local, reverse ? local : remote));
                    }
                    else
                    {
                        remoteSections.Remove(remote);
                        merge.ResultSections.Add(local);
                    }
                }
                
                localSections.Clear();
            }

            var merge = new PwdMerge(localPassFile, remotePassFile);

            var localList = localPassFile.PwdData!.Select(section => section.Copy()).ToList();
            var remoteList = remotePassFile.PwdData!.Select(section => section.Copy()).ToList();
            
            FillMerge(localList, remoteList, merge, false);
            FillMerge(remoteList, localList, merge, true);

            return merge;
        }
        
        private async Task<bool> _DecryptAsync(PassFile passFile, string askPhraseFirst, string askPhraseAgain, PassFile? localPassFile = null)
        {
            if (passFile.PwdData is not null) return true;

            if (passFile.PassPhrase is not null)
            {
                if (passFile.Decrypt(silent: true).Ok)
                {
                    return true;
                }
            }

            if (localPassFile is not null)
            {
                if (passFile.Decrypt(localPassFile.PassPhrase, true).Ok)
                {
                    return true;
                }
            }

            var passPhrase = await _dialogService.AskPasswordAsync(askPhraseFirst);

            while (passPhrase.Ok && (passPhrase.Data == string.Empty || passFile.Decrypt(passPhrase.Data!).Bad))
            {
                passPhrase = await _dialogService.AskPasswordAsync(askPhraseAgain);
            }

            if (passFile.PwdData is null) return false;

            PassFileManager.TrySetPassPhrase(passFile.Id, passPhrase.Data!);
            return true;
        }
    }
}