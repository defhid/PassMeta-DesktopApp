using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFile;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities;

using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Services;

/// <inheritdoc />
/// <remarks><see cref="PassFileType.Pwd"/> supports only.</remarks>
public class PassFilePwdExportService : IPassFileExportService
{
    private readonly IPassFileCryptoService _passFileCryptoService;
    private readonly IDialogService _dialogService;
    private readonly ILogService _logger;

    /// <summary></summary>
    public PassFilePwdExportService(IPassFileCryptoService passFileCryptoService, IDialogService dialogService, ILogService logger)
    {
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
        log = nameof(PassFilePwdExportService) + ": " + log;
        if (ex is null) _logger.Error(log);
        else _logger.Error(ex, log);
    }

    /// <inheritdoc />
    public IEnumerable<ExternalFormat> SupportedFormats { get; } = new[]
    {
        ExternalFormat.PwdPassfileEncrypted,
        ExternalFormat.PwdPassfileDecrypted,
    };

    /// <inheritdoc />
    public async Task<IResult> ExportAsync(PassFile passFile, string resultFilePath)
    {
        Debug.Assert(passFile.Type == PassFileType.Pwd);

        try
        {
            var ext = Path.GetExtension(resultFilePath).ToLower();

            if (ext == ExternalFormat.PwdPassfileEncrypted.FullExtension)
            {
                return await ExportPassfileEncryptedAsync(passFile, resultFilePath);
            }

            if (ext == ExternalFormat.PwdPassfileDecrypted.FullExtension)
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
        var result = await PassFileManager.GetEncryptedDataAsync(passFile.Type, passFile.Id);
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
        if (passFile.PwdData is null)
        {
            var result = await PassFileManager.GetEncryptedDataAsync(passFile.Type, passFile.Id);
            if (result.Bad)
            {
                return ExporterError(result.Message ?? "Getting encrypted data");
            }

            passFile.DataEncrypted = result.Data!;
            if (!await _DecryptAsync(passFile))
            {
                return Result.Failure();
            }
        }

        try
        {
            var data = PassFileConvention.Convert.FromRaw(passFile.PwdData!, true);
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

        if (passFile.PwdData is null) return false;

        PassFileManager.TrySetPassPhrase(passFile.Id, passPhrase.Data!);
        return true;
    }
}