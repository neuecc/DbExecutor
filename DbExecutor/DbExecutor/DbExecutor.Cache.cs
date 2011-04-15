using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Codeplex.Data.Extensions;

namespace Codeplex.Data
{
    public partial class DbExecutor : IDisposable
    {
        static readonly Dictionary<Type, PropertyCollection> propertyCache = new Dictionary<Type, PropertyCollection>();

        static PropertyCollection GetAccessors(Type targetType)
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
}