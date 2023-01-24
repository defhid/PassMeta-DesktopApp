using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassFileContentSerializer;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Services;

/// <inheritdoc />
/// <remarks><see cref="PassFileType.Pwd"/> supports only.</remarks>
public class PassFileExportService : IPassFileExportService
{
    private readonly IPassFileContentSerializerFactory _contentSerializerFactory;
    private readonly IPassFileCryptoService _passFileCryptoService;
    private readonly IDialogService _dialogService;
    private readonly ILogService _logger;

    /// <summary></summary>
    public PassFileExportService(
        IPassFileContentSerializerFactory contentSerializerFactory,
        IPassFileCryptoService passFileCryptoService,
        IDialogService dialogService,
        ILogService logger)
    {
        _contentSerializerFactory = contentSerializerFactory;
        _passFileCryptoService = passFileCryptoService;
        _dialogService = dialogService;
        _logger = logger;
    }

    private IResult ExporterSuccess(PassFile passFile, string resultFilePath)
    {
        _logger.Info($"{passFile} exported to '{resultFilePath}'");
        return Result.Success();
    }
        
    private IResult ExporterError(string log, Exception? ex = null)
    {
        LogError(log, ex);
        _dialogService.ShowFailure(Resources.PASSIMPORT__ERR, more: ex?.Message ?? log);
        return Result.Failure();
    }

    private void LogError(string log, Exception? ex = null)
    {
        log = nameof(PassFileExportService) + ": " + log;
        if (ex is null) _logger.Error(log);
        else _logger.Error(ex, log);
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
            var ext = Path.GetExtension(resultFilePath).TrimStart().ToLower();

            if (ext == PassFileExternalFormat.Encrypted.Extension)
            {
                return await ExportPassfileEncryptedAsync(passFile, resultFilePath);
            }

            if (ext == PassFileExternalFormat.Decrypted.Extension)
            {
                return await ExportPassfileDecryptedAsync(passFile, resultFilePath);
            }

            return ExporterError(Resources.PASSEXPORT__NOT_SUPPORTED_EXTENSION_ERR);
        }
        catch (Exception ex)
        {
            return ExporterError("Export error", ex);
        }
    }
        
    private async Task<IResult> ExportPassfileEncryptedAsync(PassFile passFile, string path)
    {
        var result = await .GetEncryptedDataAsync(passFile.Type, passFile.Id);
        if (result.Bad)
            return ExporterError(result.Message ?? "Getting encrypted data");
            
        try
        {
            await File.WriteAllBytesAsync(path, result.Data!);
            return ExporterSuccess(passFile, path);
        }
        catch (Exception ex)
        {
            return ExporterError("Encrypted data saving", ex);
        }
    }
        
    private async Task<IResult> ExportPassfileDecryptedAsync(PassFile passFile, string path)
    {
        if (passFile.Content.Decrypted is null)
        {
            if (passFile.Content.Encrypted is null)
            {
                var result = await .GetEncryptedDataAsync(passFile.Type, passFile.Id);
                if (result.Bad)
                {
                    return ExporterError(result.Message ?? "Getting encrypted data");
                }
            }

            if (!await _DecryptAsync(passFile))
            {
                return Result.Failure();
            }
        }

        try
        {
            var data = _contentSerializerFactory.For<TContent>().Serialize(passFile.Content.Decrypted!, true);
            await File.WriteAllTextAsync(path, data, PassFileConvention.JsonEncoding);
            return ExporterSuccess(passFile, path);
        }
        catch (Exception ex)
        {
            return ExporterError("Decrypted data saving", ex);
        }
    }
        
    private async Task<bool> _DecryptAsync(PassFile passFile)
    {
        var passPhrase = await _dialogService.AskPasswordAsync(
            string.Format(Resources.PASSEXPORT__ASK_PASSPHRASE, passFile.Name));

        while (passPhrase.Ok && (
                   passPhrase.Data == string.Empty || 
                   _passFileCryptoService.Decrypt(passFile, passPhrase.Data!).Bad))
        {
            passPhrase = await _dialogService.AskPasswordAsync(
                string.Format(Resources.PASSEXPORT__ASK_PASSPHRASE_AGAIN, passFile.Name));
        }

        return passFile.PwdData is not null;
    }
}