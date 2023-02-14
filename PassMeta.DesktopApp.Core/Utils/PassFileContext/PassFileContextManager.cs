using System.Collections.Generic;
using System.Linq;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Core.Utils.PassFileContext;

/// <inheritdoc />
public class PassFileContextManager : IPassFileContextManager
{
    private readonly List<IPassFileContext> _contexts = new();

    /// <inheritdoc />
    public IPassFileContext<PwdPassFile> PwdPassFileContext { get; }

    /// <inheritdoc />
    public IPassFileContext<TxtPassFile> TxtPassFileContext { get; }

    /// <inheritdoc />
    public IPassFileContext<TPassFile> PassFileContext<TPassFile>()
        where TPassFile : PassFile
        => _contexts.OfType<IPassFileContext<TPassFile>>().First();
}