using System.Diagnostics;
using Splat;

namespace PassMeta.DesktopApp.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="IReadonlyDependencyResolver"/>.
/// </summary>
public static class ReadonlyDependencyResolverExtensions
{
    /// <summary>
    /// Get required service.
    /// </summary>
    public static TService Resolve<TService>(this IReadonlyDependencyResolver resolver)
        where TService : class
    {
        var service = resolver.GetService(typeof(TService));

        Debug.Assert(service is TService, "Cannot correctly resolve " + typeof(TService));

        return (TService)service;
    }
    
    /// <summary>
    /// Get optional service.
    /// </summary>
    public static TService? ResolveOrDefault<TService>(this IReadonlyDependencyResolver resolver)
        where TService : class
    {
        var service = resolver.GetService(typeof(TService));

        Debug.Assert(service is null or TService, "Cannot correctly resolve " + typeof(TService));

        return service as TService;
    }
}