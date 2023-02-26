using AutoMapper;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Dto.Internal;
using PassMeta.DesktopApp.Common.Models.Dto.Request;
using PassMeta.DesktopApp.Common.Models.Dto.Response;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Mapping.Entities;

/// <inheritdoc />
public class PassFileProfile : Profile
{
    /// <inheritdoc />
    public PassFileProfile()
    {
        AddClone<PwdPassFile>();
        AddClone<TxtPassFile>();

        AddLocalDto<PassFile>();
        AddLocalDto<PwdPassFile>();
        AddLocalDto<TxtPassFile>();

        AddRemoteDto<PassFile>();
        AddRemoteDto<PwdPassFile>();
        AddRemoteDto<TxtPassFile>();
    }

    private void AddClone<TPassFile>()
        where TPassFile : PassFile
    {
        CreateMap<TPassFile, TPassFile>()
            .ForMember(x => x.OriginChangeStamps, opt => opt
                .MapFrom(x => new PassFileChangeStamps
                {
                    InfoChangedOn = x.InfoChangedOn,
                    VersionChangedOn = x.VersionChangedOn,
                    Version = x.Version
                }));
    }

    private void AddLocalDto<TPassFile>()
        where TPassFile : PassFile
    {
        CreateMap<TPassFile, PassFileLocalDto>()
            .ForMember(dto => dto.OriginChangeStamps, opt => opt
                .MapFrom(x => x == null
                    ? null
                    : new PassFileLocalDto
                    {
                        InfoChangedOn = x.InfoChangedOn,
                        VersionChangedOn = x.VersionChangedOn,
                        Version = x.Version
                    }));

        CreateMap<TPassFile, PassFile>()
            .ForMember(x => x.OriginChangeStamps, opt => opt
                .MapFrom(dto => dto == null
                    ? null
                    : new PassFileChangeStamps
                    {
                        InfoChangedOn = dto.InfoChangedOn,
                        VersionChangedOn = dto.VersionChangedOn,
                        Version = dto.Version
                    }));
    }

    private void AddRemoteDto<TPassFile>()
        where TPassFile : PassFile
    {
        CreateMap<PassFileInfoDto, TPassFile>()
            .ForMember(x => x.Type, opt => opt
                .MapFrom(dto => (PassFileType) dto.TypeId));

        CreateMap<TPassFile, PassFilePostData>()
            .ForMember(dto => dto.TypeId, opt => opt
                .MapFrom(x => (int) x.Type));
        
        CreateMap<TPassFile, PassFileInfoPatchData>();
    }
}