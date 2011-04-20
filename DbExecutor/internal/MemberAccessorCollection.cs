using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Codeplex.Data.Internal
{
    [Pure]
    internal class MemberAccessorCollection : IEnumerable<IMemberAccessor>
    {
        Dictionary<string, IMemberAccessor> accessors;

        public MemberAccessorCollection(Type type)
        {
            Contract.Requires(type != null);

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                .Select(pi => new ExpressionAccessor(pi));

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.SetField)
              .Select(fi => new ExpressionAccessor(fi));

            this.accessors = props.Concat(fields).ToDictionary(a => a.Name, a => (IMemberAccessor)a);
        }

        public IMemberAccessor this[string name]
        {
            get
            {
                Contract.Requires(name != null);

                IMemberAccessor accessor;
                return accessors.TryGetValue(name, out accessor)
                    ? accessor
                    : null;
            }
        }

        public IEnumerator<IMemberAccessor> GetEnumerator()
        {
            return accessors.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}