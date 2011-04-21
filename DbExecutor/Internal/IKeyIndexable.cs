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
                throw new NotImplementedException();
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}