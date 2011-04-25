using System;
using System.Collections.Generic;
using System.Data;

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

        public static IEnumerable<dynamic> ExecuteReaderDynamic(IDbConnection connection, string query,
            object parameter = null, CommandType commandType = CommandType.Text, CommandBehavior commandBehavior = CommandBehavior.Default, char parameterSymbol = '@')
        {
            using (var exec = new DbExecutor(connection, parameterSymbol))
            {
                foreach (var item in exec.ExecuteReaderDynamic(query, parameter, commandType, commandBehavior))
                {
                    yield return item;
                }
            }
        }

        public static int ExecuteNonQuery(IDbConnection connection, string query,
            object parameter = null, CommandType commandType = CommandType.Text, char parameterSymbol = '@')
        {
            using (var exec = new DbExecutor(connection, parameterSymbol))
            {
                return exec.ExecuteNonQuery(query, parameter, commandType);
            }
        }

        public static T ExecuteScalar<T>(IDbConnection connection, string query,
            object parameter = null, CommandType commandType = CommandType.Text, char parameterSymbol = '@')
        {
            using (var exec = new DbExecutor(connection, parameterSymbol))
            {
                return exec.ExecuteScalar<T>(query, parameter, commandType);
            }
        }

        public static IEnumerable<T> Select<T>(IDbConnection connection, string query,
            object parameter = null, CommandType commandType = CommandType.Text, char parameterSymbol = '@')
            where T : new()
        {
            using (var exec = new DbExecutor(connection, parameterSymbol))
            {
                foreach (var item in exec.Select<T>(query, parameter, commandType))
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<dynamic> SelectDynamic(IDbConnection connection, string query,
            object parameter = null, CommandType commandType = CommandType.Text, char parameterSymbol = '@')
        {
            using (var exec = new DbExecutor(connection, parameterSymbol))
            {
                foreach (var item in exec.SelectDynamic(query, parameter, commandType))
                {
                    yield return item;
                }
            }
        }

        public static int Insert(IDbConnection connection, string tableName, object insertItem, char parameterSymbol = '@')
        {
            using (var exec = new DbExecutor(connection, parameterSymbol))
            {
                return exec.Insert(tableName, insertItem);
            }
        }

        public static int Update(IDbConnection connection, string tableName, object updateItem, object whereCondition, char parameterSymbol = '@')
        {
            using (var exec = new DbExecutor(connection, parameterSymbol))
            {
                return exec.Update(tableName, whereCondition, updateItem);
            }
        }

        public static int Delete(IDbConnection connection, string tableName, object whereCondition, char parameterSymbol = '@')
        {
            using (var exec = new DbExecutor(connection, parameterSymbol))
            {
                return exec.Delete(tableName, whereCondition);
            }
        }
    }
}