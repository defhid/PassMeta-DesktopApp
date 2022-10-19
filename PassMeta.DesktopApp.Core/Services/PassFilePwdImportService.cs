namespace PassMeta.DesktopApp.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Common;
    using Common.Abstractions;
    using Common.Abstractions.Services;
    using Common.Abstractions.Services.PassFile;
    using Common.Constants;
    using Common.Enums;
    using Common.Models;
    using Common.Models.Entities;
    using Common.Models.Entities.Extra;
    using Newtonsoft.Json;
    using Utils;

    /// <inheritdoc />
    /// <remarks><see cref="PassFileType.Pwd"/> supports only.</remarks>
    public class PassFilePwdImportService : IPassFileImportService
    {
        private readonly ILogService _logger = EnvironmentContainer.Resolve<ILogService>();
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();
        private readonly ICryptoService _cryptoService = EnvironmentContainer.Resolve<ICryptoService>();

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
                    var sections = JsonConvert.DeserializeObject<List<PwdSection>>(passFileData) 
                                   ?? new List<PwdSection>();
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
}