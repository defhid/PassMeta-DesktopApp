namespace PassMeta.DesktopApp.Core.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common;
    using Common.Abstractions;
    using Common.Abstractions.Services.PassFile;
    using Common.Abstractions.Utils;
    using Common.Enums;
    using Common.Models;
    using Common.Models.Dto.Request;
    using Common.Models.Entities;
    using Common.Utils.Mapping;
    using Utils.Extensions;

    /// <inheritdoc />
    public class PassFileRemoteService : IPassFileRemoteService
    {
        private readonly IPassMetaClient _passMetaClient;

        private static readonly SimpleMapper<string, string> WhatToStringMapper = new MapToResource<string>[]
        {
            new("passfile_id", () => Resources.DICT_STORAGE__PASSFILE_ID),
            new("name", () => Resources.DICT_STORAGE__PASSFILE_NAME),
            new("color", () => Resources.DICT_STORAGE__PASSFILE_COLOR),
            new("created_on", () => Resources.DICT_STORAGE__PASSFILE_CREATED_ON),
            new("smth", () => Resources.DICT_STORAGE__PASSFILE_SMTH),
            new("check_password", () => Resources.DICT_STORAGE__CHECK_PASSWORD)
        };

        /// <summary></summary>
        public PassFileRemoteService(IPassMetaClient passMetaClient)
        {
            _passMetaClient = passMetaClient;
        }

        /// <inheritdoc />
        public async Task<IResult<PassFile>> GetAsync(int passFileId)
        {
            var response = await _passMetaClient.Get(PassMetaApi.PassFile.Get(passFileId))
                .WithBadHandling()
                .ExecuteAsync<PassFile>();
            return Result.FromResponse(response);
        }
        
        /// <inheritdoc />
        public async Task<List<PassFile>?> GetListAsync(PassFileType ofType)
        {
            var response = await _passMetaClient.Get(PassMetaApi.PassFile.GetList(ofType))
                .WithBadHandling()
                .ExecuteAsync<List<PassFile>>();
            return response?.Data;
        }

        /// <inheritdoc />
        public async Task<IResult<byte[]>> GetDataAsync(int passFileId, int version)
        {
            return await _passMetaClient.Get(PassMetaApi.PassFile.GetVersion(passFileId, version))
                .WithBadHandling()
                .ExecuteRawAsync();
        }

        /// <inheritdoc />
        public async Task<OkBadResponse<PassFile>?> SaveInfoAsync(PassFile passFile)
        {
            var request = _passMetaClient.Patch(PassMetaApi.PassFile.Get(passFile.Id))
                .WithJsonBody(new PassFileInfoPatchData
                {
                    Name = passFile.Name,
                    Color = passFile.Color
                })
                .WithContext(passFile.GetTitle())
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling();

            return await request.ExecuteAsync<PassFile>();
        }

        /// <inheritdoc />
        public async Task<OkBadResponse<PassFile>?> SaveDataAsync(PassFile passFile)
        {
            var request = _passMetaClient.Post(PassMetaApi.PassFile.PostVersion(passFile.Id))
                .WithFormBody(new
                {
                    smth = passFile.DataEncrypted,
                })
                .WithContext(passFile.GetTitle())
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling();

            return await request.ExecuteAsync<PassFile>();
        }

        /// <inheritdoc />
        public async Task<IResult<PassFile>> AddAsync(PassFile passFile)
        {
            var infoResponse = await _passMetaClient.Post(PassMetaApi.PassFile.Post())
                .WithJsonBody(new PassFilePostData
                {
                    Name = passFile.Name,
                    Color = passFile.Color,
                    TypeId = passFile.TypeId,
                    CreatedOn = passFile.CreatedOn,
                })
                .WithContext(passFile.GetTitle())
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling()
                .ExecuteAsync<PassFile>();

            if (infoResponse?.Success is not true)
            {
                return Result.Failure<PassFile>();
            }

            var actualPassFile = infoResponse.Data!;
            actualPassFile.DataEncrypted = passFile.DataEncrypted;

            await SaveDataAsync(actualPassFile);

            return Result.Success(actualPassFile);
        }

        /// <inheritdoc />
        public async Task<OkBadResponse?> DeleteAsync(PassFile passFile, string accountPassword)
        {
            var request = _passMetaClient.Delete(PassMetaApi.PassFile.Delete(passFile.Id))
                .WithJsonBody(new PassFileDeleteData
                {
                    CheckPassword = accountPassword
                })
                .WithContext(passFile.GetTitle())
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling();

            return await request.ExecuteAsync();
        }
    }
}