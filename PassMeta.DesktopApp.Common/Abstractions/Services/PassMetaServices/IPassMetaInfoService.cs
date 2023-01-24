using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models.Dto.Response;

namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;

/// <summary>
/// Service for getting information about current PassMeta server.
/// </summary>
public interface IPassMetaInfoService
{
    /// <summary>
    /// Load current server information.
    /// </summary>
    Task<IResult<PassMetaInfoDto>> LoadAsync();
}