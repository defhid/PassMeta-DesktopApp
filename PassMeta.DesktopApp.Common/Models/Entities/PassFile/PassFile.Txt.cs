using System.Collections.Generic;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile;

/// <inheritdoc cref="PassFile{TContent}"/>
public class TxtPassFile : PassFile<List<TxtSection>>
{
    /// <inheritdoc />
    public override PassFileType Type => PassFileType.Txt;
}