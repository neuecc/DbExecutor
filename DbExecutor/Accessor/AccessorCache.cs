using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Codeplex.Data.Internal
{
    internal static class AccessorCache
    {
        static readonly Dictionary<Type, IKeyIndexed<string, IMemberAccessor>>
            cache = new Dictionary<Type, IKeyIndexed<string, IMemberAccessor>>();

        [Pure]
        public static IKeyIndexed<string, CompiledAccessor> Lookup(Type targetType)
        {
            Contract.Requires<ArgumentNullException>(targetType != null);
            Contract.Ensures(Contract.Result<IKeyIndexed<string, CompiledAccessor>>() != null);

            lock (cache)
            {
                IKeyIndexed<string, IMemberAccessor> accessors;
                if (!cache.TryGetValue(targetType, out accessors))
                {
                    var props = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                        .Select(pi => new CompiledAccessor(pi));

                    var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.SetField)
                      .Select(fi => new CompiledAccessor(fi));

                    accessors = KeyIndexed.Create(props.Concat(fields), a => a.MemberName, a => a);
                    cache.Add(targetType, accessors);
                };

                Contract.Assume(accessors != null);
                return (IKeyIndexed<string, CompiledAccessor>)accessors;
            }
        }
    }
}