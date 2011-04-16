using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Codeplex.Data
{
    internal static class PropertyCache
    {
        static readonly Dictionary<Type, PropertyCollection> propertyCache = new Dictionary<Type, PropertyCollection>();

        public static PropertyCollection GetAccessors(Type targetType)
        {
            PropertyCollection accessors;
            if (!propertyCache.TryGetValue(targetType, out accessors))
            {
                accessors = new PropertyCollection();
                var query = targetType
                  .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                  .Select(pi => pi.ToAccessor());

                foreach (var item in query) accessors.Add(item);
                propertyCache.Add(targetType, accessors);
            };

            return accessors;
        }
    }

    internal class PropertyCollection : KeyedCollection<string, IPropertyAccessor>
    {
        protected override string GetKeyForItem(IPropertyAccessor item)
        {
            return item.Name;
        }
    }

    internal static class PropertyInfoExtensions
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