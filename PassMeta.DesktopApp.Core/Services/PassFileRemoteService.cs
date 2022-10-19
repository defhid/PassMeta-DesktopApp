namespace PassMeta.DesktopApp.Core.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common;
    using Common.Abstractions;
    using Common.Abstractions.Services.PassFile;
    using Common.Enums;
    using Common.Models;
    using Common.Models.Dto.Request;
    using Common.Models.Entities;
    using Common.Utils.Mapping;
    using Utils;
    using Utils.Extensions;

    /// <inheritdoc />
    public class PassFileRemoteService : IPassFileRemoteService
    {
        private static readonly SimpleMapper<string, string> WhatToStringMapper = new MapToResource<string>[]
        {
            new("passfile_id", () => Resources.DICT_STORAGE__PASSFILE_ID),
            new("name", () => Resources.DICT_STORAGE__PASSFILE_NAME),
            new("color", () => Resources.DICT_STORAGE__PASSFILE_COLOR),
            new("created_on", () => Resources.DICT_STORAGE__PASSFILE_CREATED_ON),
            new("smth", () => Resources.DICT_STORAGE__PASSFILE_SMTH),
            new("check_password", () => Resources.DICT_STORAGE__CHECK_PASSWORD)
        };

        /// <inheritdoc />
        public async Task<IResult<PassFile>> GetAsync(int passFileId)
        {
            var response = await PassMetaApi.GetAsync<PassFile>($"passfiles/{passFileId}", true);
            return Result.FromResponse(response);
        }
        
        /// <inheritdoc />
        public async Task<List<PassFile>?> GetListAsync(PassFileType ofType)
        {
            var response = await PassMetaApi.GetAsync<List<PassFile>>($"passfiles?type_id={(int)ofType}", true);
            return response?.Data;
        }

        /// <inheritdoc />
        public Task<OkBadResponse<string>?> GetDataAsync(int passFileId, int version)
        {
            return PassMetaApi.GetAsync<string>($"passfiles/{passFileId}/versions/{version}", true);
        }

        /// <inheritdoc />
        public Task<OkBadResponse<PassFile>?> SaveInfoAsync(PassFile passFile)
        {
            var request = PassMetaApi.Patch($"passfiles/{passFile.Id}/info", new PassFileInfoPatchData
            {
                Name = passFile.Name,
                Color = passFile.Color
            });

            return request.WithContext(passFile.GetTitle())
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling()
                .ExecuteAsync<PassFile>();
        }
        
        /// <inheritdoc />
        public Task<OkBadResponse<PassFile>?> SaveDataAsync(PassFile passFile)
        {
            var request = PassMetaApi.Post($"passfiles/{passFile.Id}/versions/new", new PassFileVersionPostData
            {
                DataEncrypted = passFile.DataEncrypted!
            });

            return request.WithContext(passFile.GetTitle())
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling()
                .ExecuteAsync<PassFile>();
        }

        /// <inheritdoc />
        public Task<OkBadResponse<PassFile>?> AddAsync(PassFile passFile)
        {
            var request = PassMetaApi.Post("passfiles/new", new PassFilePostData
            {
                Name = passFile.Name,
                Color = passFile.Color,
                TypeId = passFile.TypeId,
                CreatedOn = passFile.CreatedOn,
                Smth = passFile.DataEncrypted!
            });

            return request.WithContext(passFile.GetTitle())
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling()
                .ExecuteAsync<PassFile>();
        }

        /// <inheritdoc />
        public Task<OkBadResponse?> DeleteAsync(PassFile passFile, string accountPassword)
        {
            var request = PassMetaApi.Delete($"passfiles/{passFile.Id}", new PassFileDeleteData
            {
                CheckPassword = accountPassword
            });
                
            return request.WithContext(passFile.GetTitle())
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling()
                .ExecuteAsync();
        }
    }
}