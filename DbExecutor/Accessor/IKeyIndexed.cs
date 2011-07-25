using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Codeplex.Data.Internal
{
    [ContractClass(typeof(IKeyIndexedContract<,>))]
    internal partial interface IKeyIndexed<in TKey, out TValue> : IEnumerable<TValue>
    {
        TValue this[TKey key] { get; }
    }

    [ContractClassFor(typeof(IKeyIndexed<,>))]
    abstract class IKeyIndexedContract<TKey, TValue> : IKeyIndexed<TKey, TValue>
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