using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Codeplex.Data.Internal
{
    [ContractClass(typeof(IKeyIndexableContract<,>))]
    internal partial interface IKeyIndexable<in TKey, out TValue> : IEnumerable<TValue>
    {
        TValue this[TKey key] { get; }
    }

    [ContractClassFor(typeof(IKeyIndexable<,>))]
    abstract class IKeyIndexableContract<TKey, TValue> : IKeyIndexable<TKey, TValue>
    {
        public TValue this[TKey key]
        {
            get
            {
                Contract.Requires<ArgumentNullException>(key != null);
                return default(TValue);
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return null;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return null;
        }
    }
}