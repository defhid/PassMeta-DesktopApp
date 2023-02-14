using System;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassFileContentSerializer;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Services.PassFileServices;

/// <inheritdoc />
public class PassFileCryptoService : IPassFileCryptoService
{
    private static IDetailedResult DecryptionError => Result.Failure(Resources.PASSFILE__DECRYPTION_ERROR);
    private static IDetailedResult EncryptionError => Result.Failure(Resources.PASSFILE__ENCRYPTION_ERROR);

    private readonly IPassMetaCryptoService _pmCryptoService;
    private readonly IPassFileContentSerializerFactory _contentSerializerFactory;
    private readonly ILogsWriter _logger;

    /// <summary></summary>
    public PassFileCryptoService(
        IPassMetaCryptoService pmCryptoService,
        IPassFileContentSerializerFactory contentSerializerFactory,
        ILogsWriter logger)
    {
        _pmCryptoService = pmCryptoService;
        _contentSerializerFactory = contentSerializerFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public IDetailedResult Decrypt<TContent>(PassFile<TContent> passFile, string? passPhrase = null, bool silent = false)
        where TContent : class, new()
    {
        passPhrase ??= passFile.Content.PassPhrase;

        if (string.IsNullOrEmpty(passPhrase))
        {
            _logger.Error("Using Decrypt method without key phrase! " +
                          $"(passfile #{passFile.Id}, v{passFile.Version})");
            return DecryptionError;
        }

        if (passFile.Content.Encrypted is null)
        {
            _logger.Error("Using Decrypt method without encrypted data! " +
                          $"(passfile #{passFile.Id}, v{passFile.Version})");
            return DecryptionError;
        }

        byte[] contentBytes;
        try
        {
            contentBytes = _pmCryptoService.Decrypt(passFile.Content.Encrypted, passPhrase);
        }
        catch (Exception ex)
        {
            _logger.Debug("Silent passfile #{Id} v{Version} decryption failed: {Ex}", 
                passFile.Id, passFile.Version, ex.ToString());

            if (!silent)
            {
                _logger.Warning($"Passfile #{passFile.Id} v{passFile.Version} decryption failed: {ex.Message}");
            }

            return Result.Failure(Resources.PASSFILE__VALIDATION__WRONG_PASSPHRASE);
        }

        try
        {
            var contentRaw = _contentSerializerFactory.For<TContent>().Deserialize(contentBytes);
            passFile.Content = new PassFileContent<TContent>(contentRaw, passPhrase);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Passfile deserializing failed");
            return DecryptionError;
        }

        return Result.Success();
    }

    /// <inheritdoc />
    public IDetailedResult Encrypt<TContent>(PassFile<TContent> passFile, string? passPhrase = null)
        where TContent : class, new()
    {
        passPhrase ??= passFile.Content.PassPhrase;

        if (string.IsNullOrEmpty(passPhrase))
        {
            _logger.Error("Using Encrypt method without key phrase! " +
                          $"(passfile #{passFile.Id}, v{passFile.Version})");
            return EncryptionError;
        }

        if (passFile.Content.Decrypted is null)
        {
            _logger.Error("Using Encrypt method without decrypted data! " +
                          $"(passfile #{passFile.Id}, v{passFile.Version})");
            return EncryptionError;
        }

        byte[] contentBytes;
        try
        {
            contentBytes = _contentSerializerFactory.For<TContent>().Serialize(passFile.Content.Decrypted, false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Passfile #{passFile.Id}, v{passFile.Version} deserializing failed");
            return EncryptionError;
        }

        try
        {
            var encryptedBytes = _pmCryptoService.Encrypt(contentBytes, passPhrase);
            passFile.Content = new PassFileContent<TContent>(encryptedBytes, passPhrase);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Passfile #{passFile.Id}, v{passFile.Version} encryption failed");
            return EncryptionError;
        }

        return Result.Success();
    }
}