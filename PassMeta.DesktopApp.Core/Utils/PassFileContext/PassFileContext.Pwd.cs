using System.Collections.Generic;
using AutoMapper;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

namespace PassMeta.DesktopApp.Core.Utils.PassFileContext;

/// <inheritdoc />
public sealed class PwdPassFileContext : PassFileContext<PwdPassFile, List<PwdSection>>
{
    /// <summary></summary>
    public PwdPassFileContext(
        IPassFileLocalStorage pfLocalStorage,
        IPassFileCryptoService pfCryptoService,
        ICounter counter,
        IMapper mapper,
        IUserContext userContext,
        IDialogService dialogService,
        ILogService logger) : base(pfLocalStorage, pfCryptoService, counter, mapper, userContext, dialogService, logger)
    {
    }

    /// <inheritdoc />
    public override PassFileType PassFileType => PassFileType.Pwd;
}