namespace PassMeta.DesktopApp.Common.Utils.Mapping
{
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces.Mapping;

    /// <summary>
    /// Mapping by resource getters.
    /// </summary>
    public class ResourceMapper : IMapper
    {
        private readonly Dictionary<string, MapToResource> _mappings;

        /// <summary></summary>
        public ResourceMapper(IEnumerable<MapToResource> mappings)
        {
            _mappings = mappings.ToDictionary(map => map.From, map => map);
        }

        /// <inheritdoc />
        public string? Map(string? value)
        {
            if (value is null) return null;
            
            return _mappings.TryGetValue(value, out var mapping) ? mapping.To : value;
        }

        /// <inheritdoc />
        public IEnumerable<IMapping> GetMappings() => _mappings.Values;

        /// <summary>
        /// Create <see cref="ResourceMapper"/> from <see cref="MapToResource"/> array.
        /// </summary>
        public static implicit operator ResourceMapper(MapToResource[] mappings) => new(mappings);

        /// <summary>
        /// Create new <see cref="ResourceMapper"/> as combination
        /// of source <paramref name="mapper"/> and additional <paramref name="mappings"/>.
        /// </summary>
        public static ResourceMapper operator +(ResourceMapper mapper, MapToResource[] mappings) 
            => new(mapper._mappings.Values.Concat(mappings));
    }
}