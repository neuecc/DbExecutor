using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Codeplex.Data
{
    public partial class DbExecutor : IDisposable
    {
        /// <summary>Executes and returns the data records, when done dispose connection."</summary>
        public static IEnumerable<IDataRecord> ExecuteReader(IDbConnection connection, string query, object parameter = null)
        {
            using (var exec = new DbExecutor(connection))
            {
                foreach (var item in exec.ExecuteReader(query, parameter))
                {
                    yield return item;
                }
            }
        }

        public static int ExecuteNonQuery(IDbConnection connection, string query, object parameter = null)
        {
            using (var exec = new DbExecutor(connection))
            {
                return exec.ExecuteNonQuery(query, parameter);
            }
        }
    }
}