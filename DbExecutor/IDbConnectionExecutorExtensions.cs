using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace Codeplex.Data.Extensions
{
    public static class IDbConnectionExecutorExtensions
    {
        /// <summary>Executes and returns the data records, when done dispose connection."</summary>
        public static IEnumerable<IDataRecord> ExecuteReader(this IDbConnection connection, string query, object parameter = null)
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
            if (connection == null) throw new ArgumentNullException("connection");
            if (string.IsNullOrEmpty(query)) throw new ArgumentException("query");
            Contract.EndContractBlock();



            using (var exec = new DbExecutor(connection))
            {
                return exec.ExecuteNonQuery(query, parameter);
            }
        }
    }
}