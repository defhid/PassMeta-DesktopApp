using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassFileContentSerializer;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Core.Services.PassFileServices;

/// <inheritdoc />
public class PassFileExportService : IPassFileExportService
{
    private readonly IPassFileContentSerializerFactory _contentSerializerFactory;
    private readonly IPassFileCryptoService _passFileCryptoService;
    private readonly IPassFileLocalStorage _passFileLocalStorage;
    private readonly IUserContextProvider _userContextProvider;
    private readonly IPassPhraseAskHelper _passPhraseAskHelper;
    private readonly IDialogService _dialogService;
    private readonly ILogsWriter _logger;

    /// <summary></summary>
    public PassFileExportService(
        IPassFileContentSerializerFactory contentSerializerFactory,
        IPassFileCryptoService passFileCryptoService,
        IPassFileLocalStorage passFileLocalStorage,
        IUserContextProvider userContextProvider,
        IPassPhraseAskHelper passPhraseAskHelper,
        IDialogService dialogService,
        ILogsWriter logger)
    {
        _contentSerializerFactory = contentSerializerFactory;
        _passFileCryptoService = passFileCryptoService;
        _passFileLocalStorage = passFileLocalStorage;
        _userContextProvider = userContextProvider;
        _passPhraseAskHelper = passPhraseAskHelper;
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
    public async Task<IResult> ExportAsync<TContent>(PassFile<TContent> passFile, string resultFilePath)
        where TContent : class, new()
    {
        try
        {
            var ext = Path.GetExtension(resultFilePath).TrimStart('.');

            if (PassFileExternalFormat.Encrypted.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                return await ExportPassfileEncryptedAsync(passFile, resultFilePath);
            }

            if (PassFileExternalFormat.Decrypted.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                return await ExportPassfileDecryptedAsync(passFile, resultFilePath);
            }

            _dialogService.ShowFailure(Resources.PASSEXPORT__NOT_SUPPORTED_EXTENSION_ERR);
        }
        catch (Exception ex)
        {
            LogError($"Export {passFile.GetIdentityString()} to '{resultFilePath}' failed", ex);
            _dialogService.ShowFailure(Resources.PASSIMPORT__ERR, more: ex.Message);
        }

        return Result.Failure();
    }

    private async Task<IResult> ExportPassfileEncryptedAsync<TContent>(PassFile<TContent> passFile, string path)
        where TContent : class, new()
    {
        if (passFile.Content.Encrypted is null)
        {
            if (passFile.Content.Decrypted is not null)
            {
                var result = _passFileCryptoService.Encrypt(passFile);
                if (result.Bad)
                {
                    _dialogService.ShowFailure(result.Message!);
                    return Result.Failure();
                }
            }
            else
            {
                var result = await _passFileLocalStorage.LoadEncryptedContentAsync(passFile, _userContextProvider.Current);
                if (result.Bad)
                {
                    _dialogService.ShowFailure(result.Message!);
                    return Result.Failure();
                }
            }
        }

        try
        {
            await File.WriteAllBytesAsync(path, passFile.Content.Encrypted!);
        }
        catch (Exception ex)
        {
            LogError("Encrypted content saving failed", ex);
            throw;
        }

        LogWarning($"{passFile.GetIdentityString()} encrypted content exported to '{path}'");
        return Result.Success();
    }

    private async Task<IResult> ExportPassfileDecryptedAsync<TContent>(PassFile<TContent> passFile, string path)
        where TContent : class, new()
    {
        if (passFile.Content.Decrypted is null)
        {
            if (passFile.Content.Encrypted is null)
            {
                var result = await _passFileLocalStorage.LoadEncryptedContentAsync(passFile, _userContextProvider.Current);
                if (result.Bad)
                {
                    _dialogService.ShowFailure(result.Message!);
                    return Result.Failure();
                }
            }

            if (!await TryDecryptAsync(passFile))
            {
                return Result.Failure();
            }
        }

        try
        {
            var data = _contentSerializerFactory.For<TContent>().Serialize(passFile.Content.Decrypted!, true);
            await File.WriteAllBytesAsync(path, data);
        }
        catch (Exception ex)
        {
            LogError("Decrypted content saving failed", ex);
            throw;
        }

        LogWarning($"{passFile.GetIdentityString()} decrypted content exported to '{path}'");
        return Result.Success();
    }

    private async Task<bool> TryDecryptAsync<TContent>(PassFile<TContent> passFile)
        where TContent : class, new()
    {
        var question = string.Format(Resources.PASSEXPORT__ASK_PASSPHRASE, passFile.Name);
        var repeatQuestion = string.Format(Resources.PASSEXPORT__ASK_PASSPHRASE_AGAIN, passFile.Name);
        
        await _passPhraseAskHelper.AskLoopedAsync(question, repeatQuestion,
            x => Task.FromResult(_passFileCryptoService.Decrypt(passFile, x).Bad));

        return passFile.Content.Decrypted is not null;
    }

    private void LogError(string log, Exception? ex = null)
    {
        log = nameof(PassFileExportService) + ": " + log;
        if (ex is null) _logger.Error(log);
        else _logger.Error(ex, log);
    }

    private void LogWarning(string log)
        => _logger.Warning(nameof(PassFileExportService) + ": " + log);
}