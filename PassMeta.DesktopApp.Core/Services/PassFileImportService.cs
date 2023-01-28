using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassFileContentSerializer;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Services;

/// <inheritdoc />
public class PassFileImportService : IPassFileImportService
{
    private readonly IPassMetaCryptoService _passMetaCryptoService;
    private readonly IPassFileContentSerializerFactory _contentSerializerFactory;
    private readonly IDialogService _dialogService;
    private readonly ILogService _logger;

    /// <summary></summary>
    public PassFileImportService(
        IPassMetaCryptoService passMetaCryptoService,
        IPassFileContentSerializerFactory contentSerializerFactory,
        IDialogService dialogService,
        ILogService logger)
    {
        _passMetaCryptoService = passMetaCryptoService;
        _contentSerializerFactory = contentSerializerFactory;
        _dialogService = dialogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public IEnumerable<PassFileExternalFormat> SupportedFormats { get; } = new[]
    {
        PassFileExternalFormat.Encrypted,
        PassFileExternalFormat.Decrypted,
    };

    /// <inheritdoc />
    public async Task<IResult> ImportAsync<TContent>(PassFile<TContent> passFile, string sourceFilePath)
        where TContent : class, new()
    {
        const string oldExtension = ".old";

        try
        {
            var bytes = await File.ReadAllBytesAsync(sourceFilePath);
            var name = Path.GetFileName(sourceFilePath);
            var ext = Path.GetExtension(name);

            if (ext.Equals(oldExtension, StringComparison.OrdinalIgnoreCase))
            {
                ext = Path.GetExtension(sourceFilePath[..^oldExtension.Length]);
            }

            if (PassFileExternalFormat.Encrypted.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                return await ImportPassfileEncryptedAsync(passFile, bytes, sourceFilePath);
            }

            if (PassFileExternalFormat.Decrypted.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                return await ImportPassfileDecryptedAsync(passFile, bytes, sourceFilePath);
            }

            _dialogService.ShowFailure(Resources.PASSIMPORT__NOT_SUPPORTED_EXTENSION_ERR);
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
        var i = 0;
        while (true)
        {
            var passPhrase = await TryAskPassPhraseAsync(Path.GetFileName(path), again: ++i > 1);
            if (passPhrase is null)
                return Result.Failure();

            byte[] contentBytes;
            try
            {
                contentBytes = _passMetaCryptoService.Decrypt(fileBytes, passPhrase);
            }
            catch
            {
                LogWarning($"Passfile '{path}' decryption failed, wrong passphrase");
                continue;
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

        var passPhrase = await TryAskPassPhraseAsync(Path.GetFileName(path), askNew: true);
        if (passPhrase is null)
            return Result.Failure();

        passFile.Content = new PassFileContent<TContent>(contentRaw, passPhrase);

        LogWarning($"{passFile.GetIdentityString()} content imported from decrypted '{path}'");
        return Result.Success();
    }

    private async Task<string?> TryAskPassPhraseAsync(string fileName, bool again = false, bool askNew = false)
    {
        var passPhrase = await _dialogService.AskPasswordAsync(string.Format(again
            ? Resources.PASSIMPORT__ASK_PASSPHRASE_AGAIN
            : askNew
                ? Resources.PASSIMPORT__ASK_PASSPHRASE_NEW
                : Resources.PASSIMPORT__ASK_PASSPHRASE, fileName));

        while (passPhrase is { Ok: true, Data: "" })
        {
            passPhrase = await _dialogService.AskPasswordAsync(string.Format(askNew
                ? Resources.PASSIMPORT__ASK_PASSPHRASE_NEW_AGAIN
                : Resources.PASSIMPORT__ASK_PASSPHRASE_AGAIN, fileName));
        }

        return passPhrase.Ok ? passPhrase.Data! : null;
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