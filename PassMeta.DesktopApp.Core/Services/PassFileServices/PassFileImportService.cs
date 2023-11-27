using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassFileContentSerializer;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Core.Services.PassFileServices;

/// <inheritdoc />
public class PassFileImportService : IPassFileImportService
{
    private readonly IPassMetaCryptoService _passMetaCryptoService;
    private readonly IPassFileContentSerializerFactory _contentSerializerFactory;
    private readonly IPassPhraseAskHelper _passPhraseAskHelper;
    private readonly IDialogService _dialogService;
    private readonly ILogsWriter _logger;

    /// <summary></summary>
    public PassFileImportService(
        IPassMetaCryptoService passMetaCryptoService,
        IPassFileContentSerializerFactory contentSerializerFactory,
        IPassPhraseAskHelper passPhraseAskHelper,
        IDialogService dialogService,
        ILogsWriter logger)
    {
        _passMetaCryptoService = passMetaCryptoService;
        _contentSerializerFactory = contentSerializerFactory;
        _passPhraseAskHelper = passPhraseAskHelper;
        _dialogService = dialogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public IEnumerable<PassFileExternalFormat> GetSupportedFormats(PassFileType passFileType) => new[]
    {
        PassFileExternalFormat.Encrypted,
        PassFileExternalFormat.Decrypted,
    };

    /// <inheritdoc />
    public async Task<IResult> ImportAsync<TContent>(PassFile<TContent> passFile, string sourceFilePath)
        where TContent : class, new()
    {
        try
        {
            var bytes = await File.ReadAllBytesAsync(sourceFilePath);
            var name = Path.GetFileName(sourceFilePath);
            var ext = Path.GetExtension(name)[".".Length..];

            if (PassFileExternalFormat.Encrypted.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                return await ImportPassfileEncryptedAsync(passFile, bytes, sourceFilePath);
            }

            if (PassFileExternalFormat.Decrypted.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                return await ImportPassfileDecryptedAsync(passFile, bytes, sourceFilePath);
            }

            _dialogService.ShowFailure(string.Format(Resources.PASSIMPORT__NOT_SUPPORTED_EXTENSION_ERR, ext));
        }
        catch (Exception ex)
        {
            LogError($"Import {passFile.GetIdentityString()} from '{sourceFilePath}' failed", ex);
            _dialogService.ShowFailure(Resources.PASSIMPORT__ERR, more: ex.Message);
        }

        return Result.Failure();
    }

    private async Task<IResult> ImportPassfileEncryptedAsync<TContent>(
        PassFile<TContent> passFile,
        byte[] fileBytes,
        string path)
        where TContent : class, new()
    {
        byte[]? contentBytes = null;

        var passPhrase = await TryAskPassPhraseAsync(
            Resources.PASSIMPORT__ASK_PASSPHRASE,
            Resources.PASSIMPORT__ASK_PASSPHRASE_AGAIN, path, x => 
            {
                try
                {
                    contentBytes = _passMetaCryptoService.Decrypt(fileBytes, x);
                    return true;
                }
                catch
                {
                    LogWarning($"Passfile '{path}' decryption failed, wrong passphrase");
                    return false;
                }
            });

        if (passPhrase is null || contentBytes is null)
        {
            return Result.Failure();
        }

        try
        {
            var contentRaw = _contentSerializerFactory.For<TContent>().Deserialize(contentBytes);
            passFile.Content = new PassFileContent<TContent>(contentRaw, passPhrase);
        }
        catch (Exception ex)
        {
            LogError("Decrypted content deserializing failed", ex);
            throw;
        }

        LogWarning($"{passFile.GetIdentityString()} content imported from encrypted '{path}'");
        return Result.Success();
    }

    private async Task<IResult> ImportPassfileDecryptedAsync<TContent>(
        PassFile<TContent> passFile,
        byte[] fileBytes,
        string path)
        where TContent : class, new()
    {
        TContent contentRaw;
        try
        {
            contentRaw = _contentSerializerFactory.For<TContent>().Deserialize(fileBytes);
        }
        catch (Exception ex)
        {
            LogError("Content deserializing failed", ex);
            throw;
        }

        var passPhrase = await TryAskPassPhraseAsync(
            Resources.PASSIMPORT__ASK_PASSPHRASE_NEW,
            Resources.PASSIMPORT__ASK_PASSPHRASE_NEW_AGAIN, path);

        if (passPhrase is null)
            return Result.Failure();

        passFile.Content = new PassFileContent<TContent>(contentRaw, passPhrase);

        LogWarning($"{passFile.GetIdentityString()} content imported from decrypted '{path}'");
        return Result.Success();
    }

    private async Task<string?> TryAskPassPhraseAsync(string question, string repeatQuestion, string path,
        Func<string, bool>? validator = null)
    {
        question = string.Format(question, Path.GetFileName(path));
        repeatQuestion = string.Format(repeatQuestion, Path.GetFileName(path));

        var result = await _passPhraseAskHelper.AskLoopedAsync(question, repeatQuestion,
            x => Task.FromResult(validator?.Invoke(x) ?? true));

        return result.Data;
    }

    private void LogError(string log, Exception? ex = null)
    {
        log = nameof(PassFileImportService) + ": " + log;
        if (ex is null) _logger.Error(log);
        else _logger.Error(ex, log);
    }

    private void LogWarning(string log)
        => _logger.Warning(nameof(PassFileImportService) + ": " + log);
}