namespace PassMeta.DesktopApp.Common.Utils.Mapping
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using Abstractions.Utils.Mapping;

    /// <summary>
    /// Mapping to resource.
    /// </summary>
    public class MapToResource<TValueFrom> : IMapping<TValueFrom, string>
        where TValueFrom : notnull
    {
        private readonly TValueFrom _fromValue;
        private readonly string _toResourceName;

        /// <inheritdoc />
        public TValueFrom From => _fromValue;

        /// <inheritdoc />
        public string To => Resources.ResourceManager.GetString(_toResourceName, Resources.Culture)!;

        /// <summary>
        /// Create mapping from resource name.
        /// </summary>
        public MapToResource(TValueFrom value, string resourceName)
        {
            Debug.Assert(value is not null);
            _fromValue = value;
            _toResourceName = resourceName;
        }
        
        /// <summary>
        /// Create mapping from resource descriptor (() => Resources.SomeResourceNameForMapping).
        /// </summary>
        public MapToResource(TValueFrom value, Expression<Func<string>> resourceDescriptor) 
            : this(value, ((MemberExpression)resourceDescriptor.Body).Member.Name)
        {
        }
    }
}