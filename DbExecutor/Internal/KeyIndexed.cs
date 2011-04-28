using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Codeplex.Data.Internal
{
    internal static class KeyIndexed
    {
        public static IKeyIndexed<TKey, TElement> Create<TSource, TKey, TElement>(
            IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(keySelector != null);
            Contract.Requires<ArgumentNullException>(elementSelector != null);

            return new ReadOnlyKeyIndexedCollection<TKey, TElement>(source.ToDictionary(x => keySelector(x), x => elementSelector(x)));
        }

        [Pure]
        class ReadOnlyKeyIndexedCollection<TKey, TElement> : IKeyIndexed<TKey, TElement>
        {
            readonly Dictionary<TKey, TElement> source;

            public ReadOnlyKeyIndexedCollection(Dictionary<TKey, TElement> source)
            {
                this.source = source;
            }

            public TElement this[TKey key]
            {
                get
                {
                    TElement value;
                    return source.TryGetValue(key, out value)
                        ? value
                        : default(TElement);
                }
            }

            public IEnumerator<TElement> GetEnumerator()
            {
                return source.Values.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}