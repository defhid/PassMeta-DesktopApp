namespace PassMeta.DesktopApp.Ui.Utils.Extensions
{
    using System.Linq;
    using Avalonia.Media;
    using Common.Enums;
    using Common.Models.Entities;
    using Models.PassFile;

    /// <summary>
    /// Extension methods for <see cref="PassFile"/>.
    /// </summary>
    public static class PassFileExtension
    {
        /// <summary>
        /// Get color depending on <see cref="PassFile.IsLocalChanged"/> & <see cref="PassFile.Problem"/>.
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
        public static PassFileColor GetPassFileColor(this PassFileLight passFile)
            => PassFileColor.List.FirstOrDefault(c => c.Hex == passFile.Color?.ToUpper()) 
               ?? PassFileColor.None;
    }
}