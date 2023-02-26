using System;
using System.Threading.Tasks;

namespace PassMeta.DesktopApp.Common.Abstractions.Services;

/// <summary>
/// A service for asking user for a passphrase.
/// </summary>
public interface IPassPhraseAskHelper
{
    /// <summary>
    /// Ask for a passphrase once.
    /// </summary>
    /// <remarks>Empty phrase is always invalid.</remarks>
    Task<IResult<string>> AskAsync(string question, Func<string, Task<bool>> validator);

    /// <summary>
    /// Ask for a passphrase in a loop until phrase is valid or user cancels.
    /// </summary>
    /// <remarks>Empty phrase is always invalid.</remarks>
    Task<IResult<string>> AskLoopedAsync(string question, string repeatQuestion, Func<string, Task<bool>> validator);
}