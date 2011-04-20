using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Codeplex.Data.Internal
{
    internal static class AccessorCache
    {
        static readonly Dictionary<Type, MemberAccessorCollection> cache = new Dictionary<Type, MemberAccessorCollection>();

        public static MemberAccessorCollection Lookup(Type targetType)
        {
            Contract.Requires(targetType != null);
            Contract.Ensures(Contract.Result<MemberAccessorCollection>() != null);

            lock (cache)
            {
                MemberAccessorCollection accessors;
                if (!cache.TryGetValue(targetType, out accessors))
                {
                    accessors = new MemberAccessorCollection(targetType);
                    cache.Add(targetType, accessors);
                };

                Contract.Assume(accessors != null);
                return accessors;
            }
        }
    }
}