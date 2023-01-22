using System.Collections.Generic;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

namespace PassMeta.DesktopApp.Common.Abstractions.Entities.PassFile;

/// <summary>
/// Passfile entity with list of password sections as content.
/// </summary>
public interface IPwdPassFile : IPassFile<List<PwdSection>>
{
}