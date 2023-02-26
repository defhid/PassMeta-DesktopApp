using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Helpers;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

namespace PassMeta.DesktopApp.Core.Utils.Helpers;

/// <inheritdoc />
public class PassFileDecryptionHelper : IPassFileDecryptionHelper
{
    private readonly IPassFileCryptoService _pfCryptoService;
    private readonly IPassPhraseAskHelper _passPhraseAskHelper;

    /// <summary></summary>
    public PassFileDecryptionHelper(
        IPassFileCryptoService pfCryptoService,
        IPassPhraseAskHelper passPhraseAskHelper)
    {
        _pfCryptoService = pfCryptoService;
        _passPhraseAskHelper = passPhraseAskHelper;
    }

    /// <inheritdoc />
    public async ValueTask<IResult> ProvideDecryptedContentAsync<TPassFile>(
        TPassFile passFile,
        IPassFileContext<TPassFile> context,
        (string? Question, string? RepeatQuestion) questions = default)
        where TPassFile : PassFile
    {
        var question = questions.Question ?? Resources.PASSFILE__ASK_PASSPHRASE;
        var repeatQuestion = questions.RepeatQuestion ?? Resources.PASSFILE__ASK_PASSPHRASE_AGAIN;

        return passFile switch
        {
            PwdPassFile pwd => await ProvideDecryptedContentInternalAsync<PwdPassFile, List<PwdSection>>(
                pwd, (IPassFileContext<PwdPassFile>)context, question, repeatQuestion),
            TxtPassFile txt => await ProvideDecryptedContentInternalAsync<TxtPassFile, List<TxtSection>>(
                txt, (IPassFileContext<TxtPassFile>)context, question, repeatQuestion),
            _ => throw new ArgumentOutOfRangeException(nameof(TPassFile), typeof(TPassFile), null)
        };
    }

    private async ValueTask<IResult> ProvideDecryptedContentInternalAsync<TPassFile, TContent>(
        TPassFile passFile,
        IPassFileContext<TPassFile> context,
        string question,
        string repeatQuestion)
        where TPassFile : PassFile<TContent>
        where TContent : class, new()
    {
        if (passFile.Content.Decrypted is not null)
        {
            return Result.Success();
        }

        var provideResult = await context.ProvideEncryptedContentAsync(passFile);
        if (provideResult.Bad) return provideResult;

        if (passFile.Content.PassPhrase is not null)
        {
            var fastResult = _pfCryptoService.Decrypt(passFile, silent: true);
            if (fastResult.Ok) return fastResult;
        }

        await _passPhraseAskHelper.AskLoopedAsync(question, repeatQuestion,
            x => Task.FromResult(_pfCryptoService.Decrypt(passFile, x).Bad));

        return Result.From(passFile.Content.Decrypted is not null);
    }
}