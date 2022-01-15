namespace PassMeta.DesktopApp.Ui.Utils.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Avalonia.Media;
    using Common;
    using Common.Enums;
    using Common.Interfaces.Services;
    using Common.Models;
    using Common.Models.Entities;
    using Common.Utils.Extensions;
    using Constants;
    using Core.Utils;
    using Core.Utils.Extensions;
    using Splat;

    /// <summary>
    /// Extension methods for <see cref="PassFile"/>.
    /// </summary>
    public static class PassFileExtension
    {
        /// <summary>
        /// Get color depending on <see cref="PassFile.Problem"/> and <see cref="PassFile.Origin"/>.
        /// </summary>
        public static ISolidColorBrush GetStateColor(this PassFile? passFile)
        {
            if (passFile is null) return Brushes.LightGray;

            if (passFile.Problem is null && !passFile.LocalCreated && !passFile.LocalChanged && !passFile.LocalDeleted)
            {
                return Brushes.LightGreen;
            }

            return passFile.Problem?.Kind switch
            {
                PassFileProblemKind.NeedsMerge => Brushes.Orange,
                PassFileProblemKind.DownloadingError => Brushes.OrangeRed,
                PassFileProblemKind.UploadingError => Brushes.OrangeRed,
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
        /// Get period when passfile has been modified.
        /// </summary>
        /// <returns>String representation of zero, one or two dates.</returns>
        public static string GetPassFileChangePeriod(this PassFile passFile)
        {
            var dates = new List<DateTime>(3);

            if (passFile.LocalCreated)
            {
                dates.Add(passFile.CreatedOn);
                dates.Add(passFile.InfoChangedOn);
                dates.Add(passFile.VersionChangedOn);
            }
            else if (passFile.LocalChanged)
            {
                if (passFile.IsInformationChanged())
                {
                    dates.Add(passFile.InfoChangedOn);
                }

                if (passFile.IsVersionChanged())
                {
                    dates.Add(passFile.VersionChangedOn);
                }
            }
            else if (passFile.LocalDeleted)
            {
                dates.Add(passFile.InfoChangedOn);
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