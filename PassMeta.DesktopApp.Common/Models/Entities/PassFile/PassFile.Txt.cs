using System.Collections.Generic;
using PassMeta.DesktopApp.Common.Abstractions.Entities.PassFile;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile;

/// <inheritdoc cref="ITxtPassFile"/>
public class TxtPassFile : PassFile<List<TxtSection>>, ITxtPassFile
{
    /// <inheritdoc />
    public override PassFileType Type => PassFileType.Txt;

    /// <summary></summary>
    public TxtPassFile() : base(TxtPassFileContent.Unloaded)
    {
    }
}