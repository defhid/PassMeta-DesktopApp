namespace PassMeta.DesktopApp.Core.Utils.Mapping
{
    using Common;
    using Common.Interfaces;
    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Mapping by resource getters.
    /// </summary>
    public class ResourceMapper : IMapper
    {
        private readonly Dictionary<string, string> _mappings;

        /// <summary></summary>
        public ResourceMapper(params (string from, Expression<Func<string>> resourceDescriptor)[] mappings)
        {
            _mappings = mappings.ToDictionary(
                map => map.from, 
                map => ((MemberExpression)map.resourceDescriptor.Body).Member.Name);
        }

        /// <inheritdoc />
        public string? Map(string? value)
        {
            if (value is null) return null;
            
            return _mappings.TryGetValue(value, out var resourceName)
                ? Resources.ResourceManager.GetString(resourceName)
                : value;
        }
    }
}