namespace PassMeta.DesktopApp.Common.Utils.Mapping
{
    using System;
    using System.Linq.Expressions;
    using Interfaces.Mapping;

    /// <summary>
    /// Mapping to resource.
    /// </summary>
    public class MapToResource : IMapping
    {
        private readonly string _fromValue;
        private readonly string _toResourceName;

        /// <inheritdoc />
        public string From => _fromValue;

        /// <inheritdoc />
        public string To => Resources.ResourceManager.GetString(_toResourceName)!;

        /// <summary>
        /// Create mapping from resource name.
        /// </summary>
        public MapToResource(string value, string resourceName)
        {
            _fromValue = value;
            _toResourceName = resourceName;
        }
        
        /// <summary>
        /// Create mapping from resource descriptor (() => Resources.SomeResourceNameForMapping).
        /// </summary>
        public MapToResource(string value, Expression<Func<string>> resourceDescriptor) 
            : this(value, ((MemberExpression)resourceDescriptor.Body).Member.Name)
        {
        }
    }
}