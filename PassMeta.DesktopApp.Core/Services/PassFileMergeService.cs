namespace PassMeta.DesktopApp.Core.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Common.Interfaces;
    using Common.Interfaces.Services;
    using Common.Interfaces.Services.PassFile;
    using Common.Models;
    using Common.Models.Dto;
    using Common.Models.Entities;
    using Utils;
    using Utils.Extensions;

    /// <inheritdoc />
    public class PassFileMergeService : IPassFileMergeService
    {
        private readonly IPassFileService _passFileService= EnvironmentContainer.Resolve<IPassFileService>();
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();

        /// <inheritdoc />
        public async Task<IResult<PassFileMerge>> LoadAndPrepareMergeAsync(PassFile localPassFile)
        {
            if (localPassFile.Data is null)
            {
                var result = await PassFileManager.GetEncryptedDataAsync(localPassFile.Id);
                if (result.Bad)
                {
                    _dialogService.ShowFailure(result.Message!);
                }

                localPassFile.DataEncrypted = result.Data!;
                if (!await _DecryptAsync(localPassFile, Resources.PASSMERGE__ASK_PASSPHRASE, Resources.PASSMERGE__ASK_PASSPHRASE_AGAIN))
                {
                    return Result.Failure<PassFileMerge>();
                }
            }

            var fetchResult = await _passFileService.GetPassFileRemoteAsync(localPassFile.Id);
            if (fetchResult.Bad)
                return Result.Failure<PassFileMerge>();

            var remotePassFile = fetchResult.Data!;
            if (!await _DecryptAsync(remotePassFile, Resources.PASSMERGE__ASK_PASSPHRASE_REMOTE, Resources.PASSMERGE__ASK_PASSPHRASE_REMOTE_AGAIN))
            {
                return Result.Failure<PassFileMerge>();
            }

            var merge = _PrepareMerge(localPassFile, remotePassFile);

            return Result.Success(merge);
        }

        private static PassFileMerge _PrepareMerge(PassFile localPassFile, PassFile remotePassFile)
        {
            void FillMerge(
                IList<PassFile.Section> localSections,
                ICollection<PassFile.Section> remoteSections,
                PassFileMerge merge,
                bool reverse)
            {
                for (var i = localSections.Count - 1; i >= 0; --i)
                {
                    var local = localSections[i];
                    var remote = remoteSections.FirstOrDefault(section => section.Id == local.Id);

                    if (remote is null)
                    {
                        merge.Conflicts.Add(new PassFileMerge.Conflict(reverse ? null : local, reverse ? local : null));
                    }
                    else if (remote.Items.Count != local.Items.Count ||
                             remote.Items.Exists(it1 => 
                                 local.Items.All(it2 => it2.DiffersFrom(it1))))
                    {
                        remoteSections.Remove(remote);
                        merge.Conflicts.Add(new PassFileMerge.Conflict(reverse ? remote : local, reverse ? local : remote));
                    }
                    else
                    {
                        remoteSections.Remove(remote);
                        merge.ResultSections.Add(local);
                    }
                }
                
                localSections.Clear();
            }

            var merge = new PassFileMerge(localPassFile, remotePassFile);

            var localList = localPassFile.Data!.Select(section => section.Copy()).ToList();
            var remoteList = remotePassFile.Data!.Select(section => section.Copy()).ToList();
            
            FillMerge(localList, remoteList, merge, false);
            FillMerge(remoteList, localList, merge, true);

            return merge;
        }
        
        private async Task<bool> _DecryptAsync(PassFile passFile, string askPhraseFirst, string askPhraseAgain)
        {
            if (passFile.Data is not null) return true;
            
            var passPhrase = await _dialogService.AskPasswordAsync(askPhraseFirst);

            while (passPhrase.Ok && (passPhrase.Data == string.Empty || passFile.Decrypt(passPhrase.Data!).Bad))
            {
                passPhrase = await _dialogService.AskPasswordAsync(askPhraseAgain);
            }

            return passFile.Data is not null;
        }
    }
}