using AutoMapper;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Dto.Internal;
using PassMeta.DesktopApp.Common.Models.Dto.Request;
using PassMeta.DesktopApp.Common.Models.Dto.Response;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Utils.EntityMapping;

/// <inheritdoc />
public class PassFileProfile : Profile
{
    /// <inheritdoc />
    public PassFileProfile()
    {
        CreateMap<PassFile, PassFile>()
            .ForMember(x => x.OriginChangeStamps, opt => opt
                .MapFrom(x => new PassFileChangeStamps
                {
                    InfoChangedOn = x.InfoChangedOn,
                    VersionChangedOn = x.VersionChangedOn,
                    Version = x.Version
                }))
            .Include<PwdPassFile, PwdPassFile>()
            .Include<TxtPassFile, TxtPassFile>();

        AddLocalDto();
        AddRemoteDto();
    }

    private void AddLocalDto()
    {
        CreateMap<PassFile, PassFileLocalDto>()
            .ForMember(dto => dto.OriginChangeStamps, opt => opt
                .MapFrom(x => x == null
                    ? null
                    : new PassFileLocalDto
                    {
                        InfoChangedOn = x.InfoChangedOn,
                        VersionChangedOn = x.VersionChangedOn,
                        Version = x.Version
                    }))
            .Include<PwdPassFile, PassFileLocalDto>()
            .Include<TxtPassFile, PassFileLocalDto>();

        CreateMap<PassFileLocalDto, PassFile>()
            .ForMember(x => x.OriginChangeStamps, opt => opt
                .MapFrom(dto => dto == null
                    ? null
                    : new PassFileChangeStamps
                    {
                        InfoChangedOn = dto.InfoChangedOn,
                        VersionChangedOn = dto.VersionChangedOn,
                        Version = dto.Version
                    }))
            .Include<PassFileLocalDto, PwdPassFile>()
            .Include<PassFileLocalDto, TxtPassFile>();
    }

    private void AddRemoteDto()
    {
        CreateMap<PassFileInfoDto, PassFile>()
            .ForMember(x => x.Type, opt => opt
                .MapFrom(dto => (PassFileType) dto.TypeId))
            .Include<PassFileInfoDto, PwdPassFile>()
            .Include<PassFileInfoDto, TxtPassFile>();

        CreateMap<PassFile, PassFilePostData>()
            .ForMember(dto => dto.TypeId, opt => opt
                .MapFrom(x => (int) x.Type));
        
        CreateMap<PassFile, PassFileInfoPatchData>();
    }
}