using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Linq;

namespace Codeplex.Data.Internal
{
    internal static class AccessorCache
    {
        static readonly Dictionary<Type, IKeyIndexable<string, IMemberAccessor>>
            cache = new Dictionary<Type, IKeyIndexable<string, IMemberAccessor>>();

        public static IKeyIndexable<string, ExpressionAccessor> Lookup(Type targetType)
        {
            Contract.Requires<ArgumentNullException>(targetType != null);
            Contract.Ensures(Contract.Result<IKeyIndexable<string, ExpressionAccessor>>() != null);

            lock (cache)
            {
                IKeyIndexable<string, IMemberAccessor> accessors;
                if (!cache.TryGetValue(targetType, out accessors))
                {
                    var props = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                        .Select(pi => new ExpressionAccessor(pi));

                    var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.SetField)
                      .Select(fi => new ExpressionAccessor(fi));

                    accessors = KeyIndexable.Create(props.Concat(fields), a => a.Name, a => a);
                    cache.Add(targetType, accessors);
                };

                Contract.Assume(accessors != null);
                return (IKeyIndexable<string, ExpressionAccessor>)accessors;
            }
        }
    }
}