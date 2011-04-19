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

        public static PropertyCollection Lookup(Type targetType)
        {
            Contract.Requires(targetType != null);
            Contract.Ensures(Contract.Result<PropertyCollection>() != null);

            lock (propertyCache)
            {
                PropertyCollection accessors;
                if (!propertyCache.TryGetValue(targetType, out accessors))
                {
                    var properties = targetType
                      .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                      .Select(pi => new MemberAccessor(pi));

                    var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.SetField)
                      .Select(fi => new MemberAccessor(fi));

                    accessors = new PropertyCollection(properties.Concat(fields));
                    propertyCache.Add(targetType, accessors);
                };

                Contract.Assume(accessors != null);
                return accessors;
            }
        }
    }

    [Pure]
    internal class PropertyCollection : IEnumerable<MemberAccessor>
    {
        Dictionary<string, MemberAccessor> accessors;

        public PropertyCollection(IEnumerable<MemberAccessor> accessors)
        {
            Contract.Requires(accessors != null);

            this.accessors = accessors.ToDictionary(p => p.Name);
        }

        public MemberAccessor this[string name]
        {
            get
            {
                Contract.Requires(name != null);

                MemberAccessor accessor;
                return accessors.TryGetValue(name, out accessor)
                    ? accessor
                    : null;
            }
        }

        public IEnumerator<MemberAccessor> GetEnumerator()
        {
            return accessors.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}