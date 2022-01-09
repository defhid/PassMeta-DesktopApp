namespace PassMeta.DesktopApp.Ui.Utils.Extensions
{
    using System.Linq;
    using System.Threading.Tasks;
    using Avalonia.Media;
    using Common;
    using Common.Enums;
    using Common.Interfaces.Services;
    using Common.Models;
    using Common.Models.Entities;
    using Constants;
    using Core.Utils;
    using Splat;

    /// <summary>
    /// Extension methods for <see cref="PassFile"/>.
    /// </summary>
    public static class PassFileExtension
    {
        /// <summary>
        /// Get color depending on <see cref="PassFile.Problem"/> and <see cref="PassFile.Origin"/>.
        /// </summary>
        public static ISolidColorBrush GetStateColor(this PassFile passFile)
        {
            if (passFile.Problem is null && !passFile.LocalCreated && !passFile.LocalChanged && !passFile.LocalDeleted)
            {
                return Brushes.LightGreen;
            }

            return passFile.Problem?.Kind switch
            {
                PassFileProblemKind.NeedsMerge => Brushes.Orange,
                PassFileProblemKind.DownloadingError => Brushes.Orange,
                PassFileProblemKind.UploadingError => Brushes.Orange,
                PassFileProblemKind.Other => Brushes.Red,
                _ => Brushes.Yellow
            };
        }

        /// <summary>
        /// Parse <see cref="PassFile.Color"/> to <see cref="PassFileColor"/>.
        /// </summary>
        public static PassFileColor GetPassFileColor(this PassFile passFile)
            => PassFileColor.List.FirstOrDefault(c => c.Hex == passFile.Color?.ToUpper()) 
               ?? PassFileColor.None;

        /// <summary>
        /// Ask user for passphrase (if required), load and decode data.
        /// </summary>
        /// <remarks>Automatic show failure.</remarks>
        public static async Task<Result> LoadIfRequiredAndDecryptAsync(this PassFile passFile)
        {
            var dialogService = Locator.Current.GetService<IDialogService>()!;

            if (passFile.PassPhrase is null)
            {
                var passPhrase = await dialogService.AskPasswordAsync(Resources.PASSFILE__ASK_PASSPHRASE);
                if (passPhrase.Bad || passPhrase.Data == string.Empty)
                {
                    return Result.Failure();
                }

                passFile.PassPhrase = passPhrase.Data!;
                PassFileManager.TrySetPassPhrase(passFile.Id, passPhrase.Data!);
            }

            var result = await PassFileManager.TryLoadIfRequiredAndDecryptAsync(passFile.Id);
            if (result.Bad)
            {
                passFile.PassPhrase = null;
                PassFileManager.TrySetPassPhrase(passFile.Id, null);
                
                dialogService.ShowFailure(result.Message!);
                return Result.Failure();
            }

            passFile.Data = result.Data!;
            return Result.Success();
        }
    }
}