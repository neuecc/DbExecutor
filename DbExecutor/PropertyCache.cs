using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace Codeplex.Data
{
    internal static class PropertyCache
    {
        static readonly Dictionary<Type, PropertyCollection> propertyCache = new Dictionary<Type, PropertyCollection>();

        public static PropertyCollection GetAccessors(Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException("targetType");
            Contract.Ensures(Contract.Result<PropertyCollection>() != null);
            Contract.EndContractBlock();

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

            Contract.Assume(accessors != null);
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
            if (pi == null) throw new ArgumentNullException("pi");
            Contract.Ensures(Contract.Result<IPropertyAccessor>() != null);
            Contract.EndContractBlock();

            var func = typeof(Func<,>);
            Contract.Assume(func.IsGenericTypeDefinition);
            Contract.Assume(func.GetGenericArguments().Length == 2);

            var getterType = func.MakeGenericType(pi.DeclaringType, pi.PropertyType);
            var getMethod = pi.GetGetMethod();
            var getter = (getMethod != null) ? Delegate.CreateDelegate(getterType, getMethod) : null;

            var action = typeof(Action<,>);
            Contract.Assume(action.IsGenericTypeDefinition);
            Contract.Assume(action.GetGenericArguments().Length == 2);

            var setterType = typeof(Action<,>).MakeGenericType(pi.DeclaringType, pi.PropertyType);
            var setMethod = pi.GetSetMethod();
            var setter = (setMethod != null) ? Delegate.CreateDelegate(setterType, setMethod) : null;

            var propAccessor = typeof(PropertyAccessor<,>);
            Contract.Assume(propAccessor.IsGenericTypeDefinition);
            Contract.Assume(propAccessor.GetGenericArguments().Length == 2);

            var propertyType = propAccessor.MakeGenericType(pi.DeclaringType, pi.PropertyType);
            return (IPropertyAccessor)Activator.CreateInstance(propertyType, pi.Name, getter, setter);
        }
    }
}