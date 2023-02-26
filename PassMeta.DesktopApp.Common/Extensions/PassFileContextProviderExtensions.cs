using System;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Enums;

namespace PassMeta.DesktopApp.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="IPassFileContextProvider"/>.
/// </summary>
public static class PassFileContextProviderExtensions
{
    /// <summary>
    /// Get context for <paramref name="passFileType"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="passFileType"/> is not supported.</exception>
    public static IPassFileContext For(this IPassFileContextProvider provider, PassFileType passFileType)
    {
        foreach (var context in provider.Contexts)
        {
            if (context.PassFileType == passFileType)
            {
                return context;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(passFileType), passFileType, null);
    }

    /// <summary>
    /// Get context for <typeparamref name="TPassFile"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If <typeparamref name="TPassFile"/> is not supported.</exception>
    public static IPassFileContext<TPassFile> For<TPassFile>(this IPassFileContextProvider provider)
        where TPassFile : Models.Entities.PassFile.PassFile
    {
        foreach (var context in provider.Contexts)
        {
            if (context is IPassFileContext<TPassFile> targetContext)
            {
                return targetContext;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(TPassFile), typeof(TPassFile), null);
    }
}