using System;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Models;

namespace PassMeta.DesktopApp.Core.Services;

/// <inheritdoc />
public class PassPhraseAskHelper : IPassPhraseAskHelper
{
    private readonly IDialogService _dialogService;

    /// <summary></summary>
    public PassPhraseAskHelper(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    /// <inheritdoc />
    public async Task<IResult<string>> AskAsync(string question, Func<string, Task<bool>> validator)
    {
        var passPhrase = await _dialogService.AskPasswordAsync(question);
        if (passPhrase.Bad)
        {
            return passPhrase;
        }

        return passPhrase.Data != "" && await validator(passPhrase.Data!) 
            ? passPhrase 
            : Result.Failure<string>();
    }

    /// <inheritdoc />
    public async Task<IResult<string>> AskLoopedAsync(string question, string repeatQuestion, Func<string, Task<bool>> validator)
    {
        var passPhrase = await _dialogService.AskPasswordAsync(question);

        while (passPhrase.Ok && (passPhrase.Data == "" || !await validator(passPhrase.Data!)))
        {
            passPhrase = await _dialogService.AskPasswordAsync(repeatQuestion);
        }

        return passPhrase;
    }
}