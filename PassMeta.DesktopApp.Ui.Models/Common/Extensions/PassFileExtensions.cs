using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Common.Constants;

namespace PassMeta.DesktopApp.Ui.Models.Common.Extensions;

/// <summary>
/// Extension UI-methods for <see cref="PassFile"/>.
/// </summary>
public static class PassFileExtensions
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
}