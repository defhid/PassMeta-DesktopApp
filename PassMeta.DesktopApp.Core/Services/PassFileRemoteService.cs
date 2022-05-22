namespace PassMeta.DesktopApp.Core.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common;
    using Common.Enums;
    using Common.Interfaces;
    using Common.Interfaces.Services.PassFile;
    using Common.Models;
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
            var response = await PassMetaApi.GetAsync<List<PassFile>>($"passfiles/list?type_id={(int)ofType}", true);
            return response?.Data;
        }

        /// <inheritdoc />
        public Task<OkBadResponse<string>?> GetDataAsync(int passFileId, int? version = null)
        {
            var url = version is null
                ? $"passfiles/{passFileId}/smth"
                : $"passfiles/{passFileId}/smth?version={version}";
            
            return PassMetaApi.GetAsync<string>(url, true);
        }

        /// <inheritdoc />
        public Task<OkBadResponse<PassFile>?> SaveInfoAsync(PassFile passFile)
        {
            var request = PassMetaApi.Patch($"passfiles/{passFile.Id}/info", new
            {
                name = passFile.Name,
                color = passFile.Color
            });

            return request.WithContext(passFile.GetTitle())
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling()
                .ExecuteAsync<PassFile>();
        }
        
        /// <inheritdoc />
        public Task<OkBadResponse<PassFile>?> SaveDataAsync(PassFile passFile)
        {
            var request = PassMetaApi.Patch($"passfiles/{passFile.Id}/smth", new
            {
                smth = passFile.DataEncrypted!
            });

            return request.WithContext(passFile.GetTitle())
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling()
                .ExecuteAsync<PassFile>();
        }

        /// <inheritdoc />
        public Task<OkBadResponse<PassFile>?> AddAsync(PassFile passFile)
        {
            var request = PassMetaApi.Post("passfiles/new", new
            {
                name = passFile.Name,
                color = passFile.Color,
                type_id = passFile.TypeId,
                created_on = passFile.CreatedOn,
                smth = passFile.DataEncrypted!
            });

            return request.WithContext(passFile.GetTitle())
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling()
                .ExecuteAsync<PassFile>();
        }

        /// <inheritdoc />
        public Task<OkBadResponse?> DeleteAsync(PassFile passFile, string accountPassword)
        {
            var request = PassMetaApi.Delete($"passfiles/{passFile.Id}", new
            {
                check_password = accountPassword
            });
                
            return request.WithContext(passFile.GetTitle())
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling()
                .ExecuteAsync();
        }
    }
}