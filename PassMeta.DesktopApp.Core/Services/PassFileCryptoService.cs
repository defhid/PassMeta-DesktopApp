using System;
using System.Collections.Generic;
using Newtonsoft.Json;

using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFile;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Models.Entities.Extra;

using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Services;

/// <inheritdoc />
public class PassFileCryptoService : IPassFileCryptoService
{
    private static IDetailedResult DecryptionError => Result.Failure(Resources.PASSFILE__DECRYPTION_ERROR);
    private static IDetailedResult EncryptionError => Result.Failure(Resources.PASSFILE__ENCRYPTION_ERROR);

    private readonly ILogService _logger;

    /// <summary></summary>
    public PassFileCryptoService(ILogService logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public IDetailedResult Decrypt(PassFile passFile, string? passPhrase = null, bool silent = false)
    {
        var originPassPhrase = passFile.PassPhrase;
        if (passPhrase is not null)
        {
            passFile.PassPhrase = passPhrase;
        }

        if (string.IsNullOrEmpty(passFile.PassPhrase))
        {
            passFile.PassPhrase = originPassPhrase;
            _logger.Error("Using Decrypt method without key phrase!");
            return DecryptionError;
        }

        if (passFile.DataEncrypted is null)
        {
            passFile.PassPhrase = originPassPhrase;
            _logger.Error("Using Decrypt method without encrypted data!");
            return DecryptionError;
        }

        var service = EnvironmentContainer.Resolve<ICryptoService>();

        var content = service.Decrypt(passFile.DataEncrypted, passFile.PassPhrase, silent);
        if (content is null)
        {
            passFile.PassPhrase = originPassPhrase;
            return Result.Failure(Resources.PASSFILE__VALIDATION__WRONG_PASSPHRASE);
        }

        try
        {
            switch (passFile.Type)
            {
                case PassFileType.Pwd:
                    var json = PassFileConvention.JsonEncoding.GetString(content);
                    passFile.PwdData = JsonConvert.DeserializeObject<List<PwdSection>>(json) ?? new List<PwdSection>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(passFile.Type), passFile.Type, null);
            }
        }
        catch (Exception ex)
        {
            passFile.PassPhrase = originPassPhrase;
            _logger.Error(ex, "Passfile deserializing");
            return DecryptionError;
        }

        return Result.Success();
    }

    /// <inheritdoc />
    public IDetailedResult Encrypt(PassFile passFile)
    {
        if (string.IsNullOrEmpty(passFile.PassPhrase))
        {
            _logger.Error("Using Encrypt method without key phrase!");
            return EncryptionError;
        }

        if (passFile.PwdData is null)
        {
            _logger.Error("Using Encrypt method without decrypted data!");
            return EncryptionError;
        }

        string json;
        try
        {
            switch (passFile.Type)
            {
                case PassFileType.Pwd:
                    json = JsonConvert.SerializeObject(passFile.PwdData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(passFile.Type), passFile.Type, null);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Passfile deserializing");
            return EncryptionError;
        }

        var service = EnvironmentContainer.Resolve<ICryptoService>();

        var data = PassFileConvention.JsonEncoding.GetBytes(json);

        passFile.DataEncrypted = service.Encrypt(data, passFile.PassPhrase);
        if (passFile.DataEncrypted is null)
        {
            return EncryptionError;
        }

        return Result.Success();
    }
}