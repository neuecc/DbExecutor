using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Dynamic;

namespace Codeplex.Data
{
    public class DynamicDataRecord : DynamicObject
    {
        IDataRecord record;

        public DynamicDataRecord(IDataRecord record)
        {
            Contract.Requires<ArgumentNullException>(record != null);

            this.record = record;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            var index = indexes[0];
            result =
                (index is string) ? record[(string)index]
                : (index is int) ? record[(int)index]
                : null;
            if (result.Equals(DBNull.Value)) result = null;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = record[binder.Name];
            if (result.Equals(DBNull.Value)) result = null;
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            for (int i = 0; i < record.FieldCount; i++)
            {
                yield return record.GetName(i);
            }
        }
    }  
}