using System.Collections.Generic;
using PassMeta.DesktopApp.Common.Abstractions.Entities.PassFile;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile;

/// <inheritdoc cref="IPwdPassFile"/>
public class PwdPassFile : PassFile<List<PwdSection>>, IPwdPassFile
{
    /// <inheritdoc />
    public override PassFileType Type => PassFileType.Pwd;

    /// <summary></summary>
    public PwdPassFile() : base(PwdPassFileContent.Unloaded)
    {
    }
}