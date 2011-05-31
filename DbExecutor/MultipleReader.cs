using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using Codeplex.Data.Internal;

namespace Codeplex.Data
{
    /// <summary>Multiple DataReader</summary>
    public sealed class MultipleReader : IDisposable
    {
        IDataReader reader;

        // TODO:summary

        public bool HasNext { get; private set; }

        /// <summary>Create multiple reader.</summary>
        /// <param name="reader">Source DataReader.</param>
        public MultipleReader(IDataReader reader)
        {
            Contract.Requires<ArgumentNullException>(reader != null);

            this.reader = reader;
            HasNext = true;
        }

        private IEnumerable<IDataRecord> EnumerateReader()
        {
            try
            {
                while (reader.Read()) yield return reader;
            }
            finally
            {
                HasNext = reader.NextResult();
            }
        }

        // TODO:+index overload

        /// <summary>Returns the projection data records.</summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="selector">Data selector.</param>
        /// <returns>Result Array.</returns>
        public T[] ExecuteReader<T>(Func<IDataRecord, T> selector)
        {
            Contract.Requires<ArgumentNullException>(selector != null);
            Contract.Ensures(Contract.Result<T[]>() != null);

            return EnumerateReader().Select(selector).ToArray();
        }

        /// <summary>Returns the projection data records. The data records enclosing DynamicDataRecord.</summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="selector">Data selector.</param>
        /// <returns>Result Array.</returns>
        public T[] ExecuteReaderDynamic<T>(Func<dynamic, T> selector)
        {
            Contract.Requires<ArgumentNullException>(selector != null);
            Contract.Ensures(Contract.Result<T[]>() != null);

            var dynamicRecord = new DynamicDataRecord(reader);
            return EnumerateReader().Select(_ => selector(dynamicRecord)).ToArray();
        }

        /// <summary>Returns the first column, first row.</summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <returns>Results of first column, first row.</returns>
        public T ExecuteScalar<T>()
        {
            return (T)EnumerateReader().Select(dr => dr.GetValue(0)).FirstOrDefault();
        }

        /// <summary>Mapping objects by ColumnName - PropertyName.</summary>
        /// <typeparam name="T">Mapping target Class.</typeparam>
        /// <returns>Mapped Array.</returns>
        public T[] Select<T>() where T : new()
        {
            Contract.Ensures(Contract.Result<T[]>() != null);

            var accessors = AccessorCache.Lookup(typeof(T));
            return EnumerateReader()
                .Select(dr =>
                {
                    // if T is ValueType then can't set SetValue
                    // must be boxed
                    object result = new T();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        if (dr.IsDBNull(i)) continue;

                        var accessor = accessors[dr.GetName(i)];
                        if (accessor != null && accessor.IsWritable) accessor.SetValueDirect(result, dr[i]);
                    }
                    return (T)result;
                })
                .ToArray();
        }

        /// <summary>Mapping objects to ExpandoObject. Object is dynamic accessable by ColumnName.</summary>
        /// <returns>Mapped Array(dynamic type is ExpandoObject).</returns>
        public dynamic[] SelectDynamic()
        {
            Contract.Ensures(Contract.Result<dynamic[]>() != null);

            return EnumerateReader()
                .Select(dr =>
                {
                    IDictionary<string, object> expando = new ExpandoObject();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        var value = dr.IsDBNull(i) ? null : dr.GetValue(i);
                        expando.Add(dr.GetName(i), value);
                    }
                    return expando;
                })
                .ToArray();
        }

        // TODO:summary, Contract

        public dynamic[][] SelectAll()
        {
            var list = new List<dynamic[]>();
            while (HasNext)
            {
                list.Add(SelectDynamic());
            }
            return list.ToArray();
        }

        /// <summary>Dispose inner DataReader</summary>
        public void Dispose()
        {
            reader.Dispose();
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(reader != null);
        }

    }
}