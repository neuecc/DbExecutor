using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Linq;

namespace Codeplex.Data.Internal
{
    internal static class AccessorCache
    {
        static readonly Dictionary<Type, IKeyIndexed<string, IMemberAccessor>>
            cache = new Dictionary<Type, IKeyIndexed<string, IMemberAccessor>>();

        [Pure]
        public static IKeyIndexed<string, ExpressionAccessor> Lookup(Type targetType)
        {
            Contract.Requires<ArgumentNullException>(targetType != null);
            Contract.Ensures(Contract.Result<IKeyIndexed<string, ExpressionAccessor>>() != null);

            lock (cache)
            {
                IKeyIndexed<string, IMemberAccessor> accessors;
                if (!cache.TryGetValue(targetType, out accessors))
                {
                    var props = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                        .Select(pi => new ExpressionAccessor(pi));

                    var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.SetField)
                      .Select(fi => new ExpressionAccessor(fi));

                    accessors = KeyIndexed.Create(props.Concat(fields), a => a.Name, a => a);
                    cache.Add(targetType, accessors);
                };

                Contract.Assume(accessors != null);
                return (IKeyIndexed<string, ExpressionAccessor>)accessors;
            }
        }
    }
}