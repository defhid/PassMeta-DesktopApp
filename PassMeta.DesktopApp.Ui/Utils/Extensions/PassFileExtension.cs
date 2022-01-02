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
    using Core.Utils.Extensions;
    using Models.Constants;
    using Splat;

    /// <summary>
    /// Extension methods for <see cref="PassFile"/>.
    /// </summary>
    public static class PassFileExtension
    {
        /// <summary>
        /// Get color depending on <see cref="PassFile.IsLocalChanged"/> and <see cref="PassFile.Problem"/>.
        /// </summary>
        public static ISolidColorBrush GetStateColor(this PassFile passFile)
        {
            if (!passFile.HasProblem && !passFile.IsLocalChanged)
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
        /// Ask user for passphrase and decode data.
        /// </summary>
        /// <remarks>Automatic show failure.</remarks>
        public static async Task<Result> AskKeyPhraseAndDecryptAsync(this PassFile passFile)
        {
            if (passFile.PassPhrase is not null)
            {
                var fastResult = passFile.Decrypt();
                if (fastResult.Ok) return fastResult;
                
                passFile.PassPhrase = null;
            }
            
            var dialogService = Locator.Current.GetService<IDialogService>()!;
            
            var passPhrase = await dialogService.AskPasswordAsync(Resources.PASSFILE__ASK_PASSPHRASE);
            if (passPhrase.Bad || passPhrase.Data == string.Empty)
            {
                return Result.Failure();
            }

            passFile.PassPhrase = passPhrase.Data;
            var result = passFile.Decrypt();

            if (result.Bad)
                await dialogService.ShowFailure(result.Message!);

            return result;
        }
    }
}