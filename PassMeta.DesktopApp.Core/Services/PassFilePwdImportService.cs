using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
    
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFile;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Models.Entities.Extra;

using PassMeta.DesktopApp.Core.Services.Extensions;
using PassMeta.DesktopApp.Core.Utils;

namespace PassMeta.DesktopApp.Core.Services;

/// <inheritdoc />
/// <remarks><see cref="PassFileType.Pwd"/> supports only.</remarks>
public class PassFilePwdImportService : IPassFileImportService
{
    private readonly ICryptoService _cryptoService;
    private readonly IDialogService _dialogService;
    private readonly ILogService _logger;

    /// <summary></summary>
    public PassFilePwdImportService(ICryptoService cryptoService, IDialogService dialogService, ILogService logger)
    {
        _cryptoService = cryptoService;
        _dialogService = dialogService;
        _logger = logger;
    }

    /// <summary>
    /// Log error, show failure message and return failure result.
    /// </summary>
    private IResult<(List<PwdSection>, string)> ImporterError(string log, Exception? ex = null)
    {
        LogError(log, ex);
        _dialogService.ShowFailure(Resources.PASSIMPORT__ERR, more: ex?.Message ?? log);
        return Result.Failure<(List<PwdSection>, string)>();
    }

    private void LogError(string log, Exception? ex = null)
    {
        log = nameof(PassFilePwdImportService) + ": " + log;
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
    public async Task<IResult> ImportAsync(PassFile toPassFile, string sourceFilePath, string? supposedPassPhrase = null)
    {
        Debug.Assert(toPassFile.Type == PassFileType.Pwd);

        try
        {
            var bytes = await File.ReadAllBytesAsync(sourceFilePath);
            var name = Path.GetFileName(sourceFilePath);
            var ext = Path.GetExtension(sourceFilePath).ToLower();

            if (ext == ".old")
            {
                ext = Path.GetExtension(sourceFilePath[..^4]).ToLower();
            }

            IResult<(List<PwdSection>, string)> result;

            if (ext == ExternalFormat.PwdPassfileEncrypted.FullExtension)
            {
                result = await ImportPassfileEncryptedAsync(bytes, name, supposedPassPhrase);
            }
            else if (ext == ExternalFormat.PwdPassfileDecrypted.FullExtension)
            {
                result = await ImportPassfileDecryptedAsync(bytes, name);
            }
            else
            {
                return ImporterError(Resources.PASSIMPORT__NOT_SUPPORTED_EXTENSION_ERR);
            }

            if (result.Ok)
            {
                toPassFile.PwdData = result.Data.Item1;
                toPassFile.PassPhrase = result.Data.Item2;
            }

            return result;
        }
        catch (Exception ex)
        {
            return ImporterError("Import error", ex);
        }
    }

    private async Task<IResult<(List<PwdSection>, string)>> ImportPassfileEncryptedAsync(byte[] fileBytes, string fileName, string? supposedPassPhrase)
    {
        var i = 0;
        while (true)
        {
            var passPhrase = supposedPassPhrase ?? await _AskPassPhraseAsync(fileName, i > 0);
            if (passPhrase is null)
                return Result.Failure<(List<PwdSection>, string)>();

            ++i;
            var passFileData = _cryptoService.Decrypt(fileBytes, passPhrase);
            if (passFileData is null) continue;
                
            try
            {
                var json = PassFileConvention.JsonEncoding.GetString(passFileData);
                    
                var sections = JsonConvert.DeserializeObject<List<PwdSection>>(json) ?? new List<PwdSection>();
                return Result.Success((sections, passPhrase));
            }
            catch (Exception ex)
            {
                LogError("Passfile deserializing", ex);
            }
        }
    }
        
    private async Task<IResult<(List<PwdSection>, string)>> ImportPassfileDecryptedAsync(byte[] fileBytes, string fileName)
    {
        string passFileData;
        try
        {
            passFileData = PassFileConvention.JsonEncoding.GetString(fileBytes);
        }
        catch (Exception ex)
        {
            return ImporterError($"Converting bytes (encoding: {PassFileConvention.JsonEncoding.EncodingName})", ex);
        }

        List<PwdSection> sections;
        try
        {
            sections = PassFileConvention.Convert.ToRaw(passFileData);
        }
        catch (Exception ex)
        {
            return ImporterError("Passfile deserializing", ex);
        }

        var passPhrase = await _AskPassPhraseAsync(fileName, askNew: true);
        if (passPhrase is null)
            return Result.Failure<(List<PwdSection>, string)>();
            
        return Result.Success((sections, passPhrase));
    }
        
    private async Task<string?> _AskPassPhraseAsync(string fileName, bool again = false, bool askNew = false)
    {
        var passPhrase = await _dialogService.AskPasswordAsync(string.Format(again 
            ? Resources.PASSIMPORT__ASK_PASSPHRASE_AGAIN 
            : askNew 
                ? Resources.PASSIMPORT__ASK_PASSPHRASE_NEW 
                : Resources.PASSIMPORT__ASK_PASSPHRASE, fileName));

        while (passPhrase.Ok && passPhrase.Data == string.Empty)
        {
            passPhrase = await _dialogService.AskPasswordAsync(string.Format(askNew 
                ? Resources.PASSIMPORT__ASK_PASSPHRASE_NEW_AGAIN 
                : Resources.PASSIMPORT__ASK_PASSPHRASE_AGAIN, fileName));
        }

        return passPhrase.Ok ? passPhrase.Data! : null;
    }
}