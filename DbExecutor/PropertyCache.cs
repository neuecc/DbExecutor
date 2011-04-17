using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Collections.Concurrent;

namespace Codeplex.Data.Infrastructure
{
    internal static class PropertyCache
    {
        static readonly Dictionary<Type, PropertyCollection> propertyCache = new Dictionary<Type, PropertyCollection>();

        public static PropertyCollection GetAccessors(Type targetType)
        {
            Contract.Requires(targetType != null);
            Contract.Ensures(Contract.Result<PropertyCollection>() != null);

            lock (propertyCache)
            {
                PropertyCollection accessors;
                if (!propertyCache.TryGetValue(targetType, out accessors))
                {
                    var query = targetType
                      .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                      .Select(pi => pi.ToAccessor());

                    accessors = new PropertyCollection(query);
                    propertyCache.Add(targetType, accessors);
                };

                Contract.Assume(accessors != null);
                return accessors;
            }
        }
    }

    internal class PropertyCollection : KeyedCollection<string, IPropertyAccessor>
    {
        public PropertyCollection(IEnumerable<IPropertyAccessor> accessors)
        {
            Contract.Requires(accessors != null);

            foreach (var item in accessors) Add(item);
        }

        protected override string GetKeyForItem(IPropertyAccessor item)
        {
            return item.Name;
        }
    }

    internal static class PropertyInfoExtensions
    {
        /// <summary>Make delegate accessor.</summary>
        public static IPropertyAccessor ToAccessor(this PropertyInfo propertyInfo)
        {
            Contract.Requires(propertyInfo != null);
            Contract.Ensures(Contract.Result<IPropertyAccessor>() != null);
            Contract.Assume(typeof(Func<,>).IsGenericTypeDefinition);
            Contract.Assume(typeof(Func<,>).GetGenericArguments().Length == 2);
            Contract.Assume(typeof(Action<,>).IsGenericTypeDefinition);
            Contract.Assume(typeof(Action<,>).GetGenericArguments().Length == 2);
            Contract.Assume(typeof(PropertyAccessor<,>).IsGenericTypeDefinition);
            Contract.Assume(typeof(PropertyAccessor<,>).GetGenericArguments().Length == 2);

            var getterType = typeof(Func<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            var getMethod = propertyInfo.GetGetMethod();
            var getter = (getMethod != null) ? Delegate.CreateDelegate(getterType, getMethod) : null;

            var setterType = typeof(Action<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            var setMethod = propertyInfo.GetSetMethod();
            var setter = (setMethod != null) ? Delegate.CreateDelegate(setterType, setMethod) : null;

            var propertyType = typeof(PropertyAccessor<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (IPropertyAccessor)Activator.CreateInstance(propertyType, propertyInfo.Name, getter, setter);
        }
    }
}