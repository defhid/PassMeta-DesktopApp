namespace PassMeta.DesktopApp.Core.Utils.Extensions
{
    using System;
    using System.Linq.Expressions;
    using Common.Models.Entities;

    /// <summary>
    /// Extension methods for <see cref="PassFile"/>.
    /// </summary>
    public static class PassFileExtensions
    {
        /// <summary>
        /// Has property been changed locally?
        /// </summary>
        public static bool Changed<TPropertyValue>(this PassFile passFile, Expression<Func<PassFile, TPropertyValue>> propertyDescriptor)
        {
            if (passFile.Origin is null) return false;
            
            var exp = (MemberExpression)propertyDescriptor.Body;
            var property = typeof(PassFile).GetProperty(exp.Member.Name)!;
            
            return property.GetValue(passFile) != property.GetValue(passFile.Origin);
        }
    }
}