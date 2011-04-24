using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace Codeplex.Data
{
    public partial class DbExecutor : IDisposable
    {
        /// <summary>Executes and returns the data records, when done dispose connection."</summary>
        public static IEnumerable<IDataRecord> ExecuteReader(IDbConnection connection, string query,
            object parameter = null, CommandType commandType = CommandType.Text, CommandBehavior commandBehavior = CommandBehavior.Default, char parameterSymbol = '@')
        {
            using (var exec = new DbExecutor(connection, parameterSymbol))
            {
                foreach (var item in exec.ExecuteReader(query, parameter, commandType, commandBehavior))
                {
                    yield return item;
                }
            }
        }
    }
}