using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Helpers;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Core.Utils.Helpers;

/// <inheritdoc />
public class PassFileContentHelper<TPassFile, TContent> : IPassFileContentHelper<TPassFile>
    where TPassFile : PassFile<TContent>
    where TContent : class
{
    private readonly IPassFileDecryptionHelper _pfDecryptionHelper;
    private readonly IPassFileContextProvider _pfContextProvider;
    private readonly IDialogService _dialogService;

    /// <summary></summary>
    public PassFileContentHelper(
        IPassFileDecryptionHelper pfDecryptionHelper,
        IPassFileContextProvider pfContextProvider,
        IDialogService dialogService)
    {
        _pfDecryptionHelper = pfDecryptionHelper;
        _pfContextProvider = pfContextProvider;
        _dialogService = dialogService;
    }

    /// <inheritdoc />
    public async Task<IResult> ChangePassPhraseAsync(TPassFile passFile)
    {
        var pfContext = _pfContextProvider.For<TPassFile>();
        
        var result = await _pfDecryptionHelper.ProvideDecryptedContentAsync(passFile, pfContext, (
            Question: Resources.PASSFILE__ASK_PASSPHRASE_OLD,
            RepeatQuestion: null));
        if (result.Bad)
        {
            return result;
        }

        var provideResult = await pfContext.ProvideEncryptedContentAsync(passFile);
        if (provideResult.Bad)
        {
            return result;
        }

        var passPhraseNew = await _dialogService.AskPasswordAsync(Resources.PASSFILE__ASK_PASSPHRASE_NEW);
        if (passPhraseNew.Bad)
        {
            return result;
        }

        passFile.Content = new PassFileContent<TContent>(passFile.Content.Encrypted!, passPhraseNew.Data!);

        return pfContext.UpdateContent(passFile);
    }
}