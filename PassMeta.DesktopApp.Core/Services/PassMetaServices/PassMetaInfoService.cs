using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Dto.Response;

namespace PassMeta.DesktopApp.Core.Services.PassMetaServices;

/// <inheritdoc />
public class PassMetaInfoService : IPassMetaInfoService
{
    private readonly IPassMetaClient _passMetaClient;

    /// <summary></summary>
    public PassMetaInfoService(IPassMetaClient passMetaClient)
    {
        _passMetaClient = passMetaClient;
    }

    /// <inheritdoc />
    public async Task<IDetailedResult<PassMetaInfoDto>> LoadAsync()
    {
        var response = await _passMetaClient.Begin(PassMetaApi.General.GetInfo())
            .ExecuteAsync<PassMetaInfoDto>();

        return Result.FromResponse(response);
    }
}