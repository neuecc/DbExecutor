using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Codeplex.Data.Extensions
{
    public static class IDbCommandExtensions
    {
        /// <summary>Enumerate Rows.</summary>
        public static IEnumerable<IDataRecord> EnumerateAll(this IDbCommand command)
        {
            using (command)
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read()) yield return reader;
            }
        }
    }

    public static class PropertyInfoExtensions
    {
        /// <summary>Make delegate accessor.</summary>
        public static IPropertyAccessor ToAccessor(this PropertyInfo pi)
        {
            var getterType = typeof(Func<,>).MakeGenericType(pi.DeclaringType, pi.PropertyType);
            var getMethod = pi.GetGetMethod();
            var getter = (getMethod != null) ? Delegate.CreateDelegate(getterType, getMethod) : null;

            var setterType = typeof(Action<,>).MakeGenericType(pi.DeclaringType, pi.PropertyType);
            var setMethod = pi.GetSetMethod();
            var setter = (setMethod != null) ? Delegate.CreateDelegate(setterType, setMethod) : null;

            var propertyType = typeof(PropertyAccessor<,>).MakeGenericType(pi.DeclaringType, pi.PropertyType);
            return (IPropertyAccessor)Activator.CreateInstance(propertyType, pi.Name, getter, setter);
        }
    }
}