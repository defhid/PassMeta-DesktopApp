namespace PassMeta.DesktopApp.Core
{
    using System.Runtime.CompilerServices;
    using Splat;

    /// <summary>
    /// Environment container.
    /// </summary>
    public static class EnvironmentContainer
    {
        private static IReadonlyDependencyResolver _resolver = null!;
        
        /// <summary>
        /// Resolve service.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TService Resolve<TService>(string? contract = null) => (TService)_resolver.GetService(typeof(TService))!;

        /// <summary>
        /// Initialize environment container.
        /// </summary>
        public static void Initialize(IReadonlyDependencyResolver resolver)
        {
            _resolver = resolver;
        }
    }
}