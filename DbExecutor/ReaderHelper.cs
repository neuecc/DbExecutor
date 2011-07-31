using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using Codeplex.Data.Internal;
using System.Collections;
using System.Text;

namespace Codeplex.Data
{
    internal static class ReaderHelper
    {
        internal static T SelectCore<T>(IDataRecord dataRecord, IKeyIndexed<string, CompiledAccessor> accessors) where T : new()
        {
            // if T is ValueType then can't set SetValue
            // must be boxed
            object result = new T();
            for (int i = 0; i < dataRecord.FieldCount; i++)
            {
                if (dataRecord.IsDBNull(i)) continue;

                var accessor = accessors[dataRecord.GetName(i)];
                if (accessor != null && accessor.IsWritable) accessor.SetValueDirect(result, dataRecord[i]);
            }
            return (T)result;
        }

        internal static dynamic SelectDynamicCore(IDataRecord dataRecord)
        {
            IDictionary<string, object> expando = new ExpandoObject();
            for (int i = 0; i < dataRecord.FieldCount; i++)
            {
                var value = dataRecord.IsDBNull(i) ? null : dataRecord.GetValue(i);
                expando.Add(dataRecord.GetName(i), value);
            }
            return expando;
        }
    }
}