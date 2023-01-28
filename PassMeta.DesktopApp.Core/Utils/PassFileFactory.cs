using System;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Dto.Internal;
using PassMeta.DesktopApp.Common.Models.Dto.Response;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Core.Utils;

/// <summary>
/// Factory for passfiles.
/// </summary>
public static class PassFileFactory
{
    public static PassFile FromDto(PassFileInfoDto dto)
    {
        var passFile = CreatePassFile(
            dto.Id, dto.UserId, dto.TypeId, dto.CreatedOn, dto.InfoChangedOn, dto.VersionChangedOn);

        passFile.Name = dto.Name;
        passFile.Color = dto.Color;
        passFile.Version = dto.Version;

        return passFile;
    }
    
    public static PassFile FromDto(PassFileLocalDto dto)
    {
        var passFile = CreatePassFile(
            dto.Id, dto.UserId, (int) dto.Type, dto.CreatedOn, dto.InfoChangedOn, dto.VersionChangedOn,
            dto.LocalDeletedOn, dto.Origin is null ? null : FromDto(dto));

        passFile.Name = dto.Name;
        passFile.Color = dto.Color;
        passFile.Version = dto.Version;

        return passFile;
    }

    private static PassFile CreatePassFile(
        long id, int userId, int passFileTypeId,
        DateTime createdOn, DateTime infoChangedOn, DateTime versionChangedOn,
        DateTime? localDeletedOn = null, PassFile? origin = null)
        => passFileTypeId switch
        {
            (int) PassFileType.Pwd => new PwdPassFile
            {
                Id = id, UserId = userId, CreatedOn = createdOn, 
                InfoChangedOn = infoChangedOn, VersionChangedOn = versionChangedOn,
                LocalDeletedOn = localDeletedOn, OriginChangeStamps = origin
            },
            (int) PassFileType.Txt => new TxtPassFile
            {
                Id = id, UserId = userId, CreatedOn = createdOn,
                InfoChangedOn = infoChangedOn, VersionChangedOn = versionChangedOn,
                LocalDeletedOn = localDeletedOn, OriginChangeStamps = origin
            },
            _ => throw new ArgumentOutOfRangeException(nameof(passFileTypeId), passFileTypeId, @"Passfile type is not supported")
        };
}