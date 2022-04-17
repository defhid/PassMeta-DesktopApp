namespace PassMeta.DesktopApp.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Common;
    using Common.Constants;
    using Common.Interfaces;
    using Common.Interfaces.Services;
    using Common.Interfaces.Services.PassFile;
    using Common.Models;
    using Common.Models.Entities;
    using Newtonsoft.Json;
    using Utils;

    /// <inheritdoc />
    public class PassFileImportService : IPassFileImportService
    {
        private readonly ILogService _logger = EnvironmentContainer.Resolve<ILogService>();
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();
        private readonly ICryptoService _cryptoService = EnvironmentContainer.Resolve<ICryptoService>();

        /// <summary>
        /// Log error, show failure message and return failure result.
        /// </summary>
        private IResult<(List<PassFile.Section>, string)> ImporterError(string log, Exception? ex = null)
        {
            LogError(log, ex);
            _dialogService.ShowFailure(Resources.PASSIMPORT__ERR, more: ex?.Message ?? log);
            return Result.Failure<(List<PassFile.Section>, string)>();
        }

        private void LogError(string log, Exception? ex = null)
        {
            log = nameof(PassFileImportService) + ": " + log;
            if (ex is null) _logger.Error(log);
            else _logger.Error(ex, log);
        }

        /// <inheritdoc />
        public async Task<IResult<(List<PassFile.Section>, string)>> ImportAsync(string sourceFilePath, string? supposedPassPhrase = null)
        {
            try
            {
                var bytes = await File.ReadAllBytesAsync(sourceFilePath);
                var name = Path.GetFileName(sourceFilePath);
                var ext = Path.GetExtension(sourceFilePath).ToLower();

                if (ext == ".old")
                {
                    ext = Path.GetExtension(sourceFilePath[..^4]).ToLower();
                }

                if (ext == ExternalFormat.PassfileEncrypted.FullExtension)
                {
                    return await ImportPassfileEncryptedAsync(bytes, name, supposedPassPhrase);
                }

                if (ext == ExternalFormat.PassfileDecrypted.FullExtension)
                {
                    return await ImportPassfileDecryptedAsync(bytes, name);
                }

                return ImporterError(Resources.PASSIMPORT__NOT_SUPPORTED_EXTENSION_ERR);
            }
            catch (Exception ex)
            {
                return ImporterError("Import error", ex);
            }
        }

        private async Task<IResult<(List<PassFile.Section>, string)>> ImportPassfileEncryptedAsync(byte[] fileBytes, string fileName, string? supposedPassPhrase)
        {
            var i = 0;
            while (true)
            {
                var passPhrase = supposedPassPhrase ?? await _AskPassPhraseAsync(fileName, i > 0);
                if (passPhrase is null)
                    return Result.Failure<(List<PassFile.Section>, string)>();

                ++i;
                var passFileData = _cryptoService.Decrypt(fileBytes, passPhrase);
                if (passFileData is null) continue;
                
                try
                {
                    var sections = JsonConvert.DeserializeObject<List<PassFile.Section>>(passFileData) 
                                   ?? new List<PassFile.Section>();
                    return Result.Success((sections, passPhrase));
                }
                catch (Exception ex)
                {
                    LogError("Passfile deserializing", ex);
                }
            }
        }
        
        private async Task<IResult<(List<PassFile.Section>, string)>> ImportPassfileDecryptedAsync(byte[] fileBytes, string fileName)
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

            List<PassFile.Section> sections;
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
                return Result.Failure<(List<PassFile.Section>, string)>();
            
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
}