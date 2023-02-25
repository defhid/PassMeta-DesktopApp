using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Constants;

namespace PassMeta.DesktopApp.Ui.Extensions;

/// <summary>
/// Extension UI-methods for <see cref="PassFile"/>.
/// </summary>
public static class PassFileUiExtensions
{
    /// <summary>
    /// Get color depending on <see cref="PassFile.Mark"/> and <see cref="PassFile.OriginChangeStamps"/>.
    /// </summary>
    public static ISolidColorBrush GetStateColor(this PassFile? passFile)
    {
        if (passFile is null) return Brushes.LightGray;

        if (passFile.Mark is PassFileMark.None && !passFile.IsLocalChanged())
        {
            return Brushes.WhiteSmoke;
        }

        return passFile.Mark switch
        {
            PassFileMark.NeedsMerge => Brushes.Orange,
            PassFileMark.DownloadingError => Brushes.OrangeRed,
            PassFileMark.UploadingError => Brushes.OrangeRed,
            PassFileMark.RemoteDeletingError => Brushes.OrangeRed,
            PassFileMark.OtherError => Brushes.Red,
            _ => passFile.IsLocalDeleted() ? Brushes.LightGray : Brushes.Yellow
        };
    }

    /// <summary>
    /// Parse <see cref="PassFile.Color"/> to <see cref="PassFileColor"/>.
    /// </summary>
    public static PassFileColor GetPassFileColor(this PassFile passFile)
        => PassFileColor.List.FirstOrDefault(c => c.Hex == passFile.Color?.ToUpper()) 
           ?? PassFileColor.None;

    /// <summary>
    /// Get period when passfile has been modified.
    /// </summary>
    /// <returns>String representation of zero, one or two dates.</returns>
    public static string GetPassFileChangePeriod(this PassFile passFile)
    {
        var dates = new List<DateTime>(3);

        if (passFile.IsLocalCreated())
        {
            dates.Add(passFile.CreatedOn);
            dates.Add(passFile.InfoChangedOn);
            dates.Add(passFile.VersionChangedOn);
        }
        else if (passFile.IsLocalDeleted())
        {
            dates.Add(passFile.DeletedOn!.Value);
        }
        else if (passFile.IsLocalChanged())
        {
            if (passFile.IsLocalInfoFieldsChanged())
            {
                dates.Add(passFile.InfoChangedOn);
            }

            if (passFile.IsLocalVersionFieldsChanged())
            {
                dates.Add(passFile.VersionChangedOn);
            }
        }

        dates.Sort();
        if (dates.Count > 2)
        {
            dates = new List<DateTime>(2)
            {
                dates.First(),
                dates.Last()
            };
        }

        return string.Join(" - ", dates.Distinct().Select(d => d.ToShortDateTimeString()));
    }

    /// <summary>
    /// Ask user for passphrase (if required), load and decode data.
    /// </summary>
    /// <remarks>Automatic show failure.</remarks>
    public static async Task<IResult> LoadIfRequiredAndDecryptAsync(this PassFile passFile, IDialogService dialogService, bool askOld = false)
    {
        passFile.PassPhrase ??= PassFileManager.GetPassPhrase(passFile.Id);
            
        if (passFile.PassPhrase is null)
        {
            var passPhrase = await dialogService.AskPasswordAsync(askOld 
                ? Resources.PASSFILE__ASK_PASSPHRASE_OLD
                : Resources.PASSFILE__ASK_PASSPHRASE);

            if (passPhrase.Bad || passPhrase.Data == string.Empty)
            {
                return Result.Failure();
            }

            passFile.PassPhrase = passPhrase.Data!;
            PassFileManager.TrySetPassPhrase(passFile.Id, passPhrase.Data!);
        }

        var result = await PassFileManager.TryLoadIfRequiredAndDecryptAsync(passFile);
        if (result.Ok) return result;

        passFile.PassPhrase = null;
        PassFileManager.TrySetPassPhrase(passFile.Id, null);
                
        dialogService.ShowFailure(result.Message!);
        return Result.Failure();
    }
        
    private static async Task<IResult> TryLoadIfRequiredAndDecryptAsync<TPassFile, TContent>(
        this IPassFileContext<TPassFile> context, 
        TPassFile passFile,
        IPassFileCryptoService pfCryptoService,
        IDialogService dialogService)
        where TPassFile : PassFile<TContent>
        where TContent : class, new()
    {
        if (passFile.Content.Decrypted is not null)
        {
            return Result.Success();
        }

        if (passFile.Content.Encrypted is null)
        {
            var loadResult = await context.LoadEncryptedContentAsync(passFile);
            if (loadResult.Bad)
            {
                return loadResult;
            }
        }

        var decryptResult = pfCryptoService.Decrypt(passFile);
        if (decryptResult.Bad)
        {
            dialogService.ShowError(decryptResult.Message!);
        }

        return decryptResult;
    }
}