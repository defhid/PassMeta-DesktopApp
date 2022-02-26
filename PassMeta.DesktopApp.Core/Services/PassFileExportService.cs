namespace PassMeta.DesktopApp.Core.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Common;
    using Common.Constants;
    using Common.Interfaces.Services;
    using Common.Interfaces.Services.PassFile;
    using Common.Models;
    using Common.Models.Entities;
    using Utils;
    using Utils.Extensions;

    /// <inheritdoc />
    public class PassFileExportService : IPassFileExportService
    {
        private readonly ILogService _logger = EnvironmentContainer.Resolve<ILogService>();
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();

        private Result ExporterSuccess(PassFile passFile, string resultFilePath)
        {
            _logger.Info($"{passFile} exported to '{resultFilePath}'");
            return Result.Success();
        }
        
        private Result ExporterError(string log, Exception? ex = null)
        {
            LogError(log, ex);
            _dialogService.ShowFailure(Resources.PASSIMPORT__ERR, more: ex?.Message ?? log);
            return Result.Failure();
        }

        private void LogError(string log, Exception? ex = null)
        {
            log = nameof(PassFileImportService) + ": " + log;
            if (ex is null) _logger.Error(log);
            else _logger.Error(ex, log);
        }
        
        /// <inheritdoc />
        public async Task<Result> ExportAsync(PassFile passFile, string resultFilePath)
        {
            try
            {
                var ext = Path.GetExtension(resultFilePath).ToLower();

                if (ext == ExternalFormat.PassfileEncrypted.FullExtension)
                {
                    return await ExportPassfileEncryptedAsync(passFile, resultFilePath);
                }

                if (ext == ExternalFormat.PassfileDecrypted.FullExtension)
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
        
        private async Task<Result> ExportPassfileEncryptedAsync(PassFile passFile, string path)
        {
            var result = await PassFileManager.GetEncryptedDataAsync(passFile.Id);
            if (result.Bad)
                return ExporterError(result.Message ?? "Getting encrypted data");
            
            try
            {
                await File.WriteAllBytesAsync(path, PassFileConvention.Convert.EncryptedStringToBytes(result.Data!));
                return ExporterSuccess(passFile, path);
            }
            catch (Exception ex)
            {
                return ExporterError("Encrypted data saving", ex);
            }
        }
        
        private async Task<Result> ExportPassfileDecryptedAsync(PassFile passFile, string path)
        {
            if (passFile.Data is null)
            {
                var result = await PassFileManager.GetEncryptedDataAsync(passFile.Id);
                if (result.Bad)
                {
                    return ExporterError(result.Message ?? "Getting encrypted data");
                }

                passFile.DataEncrypted = result.Data!;
                if (!await _AskPassPhraseAndDecryptAsync(passFile))
                {
                    return Result.Failure();
                }
            }

            try
            {
                var data = PassFileConvention.Convert.FromRaw(passFile.Data!);
                await File.WriteAllTextAsync(path, data, PassFileConvention.JsonEncoding);
                return ExporterSuccess(passFile, path);
            }
            catch (Exception ex)
            {
                return ExporterError("Decrypted data saving", ex);
            }
        }
        
        private async Task<Result> _AskPassPhraseAndDecryptAsync(PassFile passFile)
        {
            bool TryDecrypt(string passPhrase)
            {
                passFile.PassPhrase = passPhrase;
                return passFile.Decrypt().Ok;
            }

            var passPhrase = await _dialogService.AskPasswordAsync(
                string.Format(Resources.PASSEXPORT__ASK_PASSPHRASE, passFile.Name));

            while (passPhrase.Ok && (passPhrase.Data == string.Empty || !TryDecrypt(passPhrase.Data!)))
            {
                passPhrase = await _dialogService.AskPasswordAsync(
                    string.Format(Resources.PASSEXPORT__ASK_PASSPHRASE_AGAIN, passFile.Name));
            }
            
            return passPhrase.WithoutData();
        }
    }
}