using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;

namespace Codeplex.Data
{
    public partial class DbExecutor : IDisposable
    {
        static IEnumerable<IDataRecord> ExecuteReaderHelper(IDbConnection connection, string query, object parameter, CommandType commandType, CommandBehavior commandBehavior)
        {
            using (var exec = new DbExecutor(connection))
            {
                foreach (var item in exec.ExecuteReader(query, parameter, commandType, commandBehavior))
                {
                    yield return item;
                }
            }
        }

        /// <summary>Executes and returns the data records, when done dispose connection.<para>When done dispose connection.</para></summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="query">SQL code.</param>
        /// <param name="parameter">PropertyName parameterized to PropertyName. if null then no use parameter.</param>
        /// <param name="commandType">Command Type.</param>
        /// <param name="commandBehavior">Command Behavior.</param>
        /// <returns>Query results.</returns>
        public static IEnumerable<IDataRecord> ExecuteReader(IDbConnection connection, string query,
            object parameter = null, CommandType commandType = CommandType.Text, CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(query));
            Contract.Ensures(Contract.Result<IEnumerable<IDataRecord>>() != null);

            return ExecuteReaderHelper(connection, query, parameter, commandType, commandBehavior);
        }

        static IEnumerable<dynamic> ExecuteReaderDynamicHelper(IDbConnection connection, string query, object parameter, CommandType commandType, CommandBehavior commandBehavior)
        {
            using (var exec = new DbExecutor(connection))
            {
                foreach (var item in exec.ExecuteReaderDynamic(query, parameter, commandType, commandBehavior))
                {
                    yield return item;
                }
            }
        }

        /// <summary>Executes and returns the data records enclosing DynamicDataRecord.<para>When done dispose connection.</para></summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="query">SQL code.</param>
        /// <param name="parameter">PropertyName parameterized to PropertyName. if null then no use parameter.</param>
        /// <param name="commandType">Command Type.</param>
        /// <param name="commandBehavior">Command Behavior.</param>
        /// <returns>Query results. Result type is DynamicDataRecord.</returns>
        public static IEnumerable<dynamic> ExecuteReaderDynamic(IDbConnection connection, string query,
            object parameter = null, CommandType commandType = CommandType.Text, CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(query));
            Contract.Ensures(Contract.Result<IEnumerable<dynamic>>() != null);

            return ExecuteReaderDynamicHelper(connection, query, parameter, commandType, commandBehavior);
        }

        /// <summary>Executes and returns the number of rows affected.<para>When done dispose connection.</para></summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="query">SQL code.</param>
        /// <param name="parameter">PropertyName parameterized to PropertyName. if null then no use parameter.</param>
        /// <param name="commandType">Command Type.</param>
        /// <returns>Rows affected.</returns>
        public static int ExecuteNonQuery(IDbConnection connection, string query,
            object parameter = null, CommandType commandType = CommandType.Text)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(query));

            using (var exec = new DbExecutor(connection))
            {
                return exec.ExecuteNonQuery(query, parameter, commandType);
            }
        }

        /// <summary>Executes and returns the first column, first row.<para>When done dispose connection.</para></summary>
        /// <param name="connection">Database connection.</param>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="query">SQL code.</param>
        /// <param name="parameter">PropertyName parameterized to PropertyName. if null then no use parameter.</param>
        /// <param name="commandType">Command Type.</param>
        /// <returns>Query results of first column, first row.</returns>
        public static T ExecuteScalar<T>(IDbConnection connection, string query,
            object parameter = null, CommandType commandType = CommandType.Text)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(query));

            using (var exec = new DbExecutor(connection))
            {
                return exec.ExecuteScalar<T>(query, parameter, commandType);
            }
        }

        static IEnumerable<T> SelectHelper<T>(IDbConnection connection, string query, object parameter, CommandType commandType)
            where T : new()
        {
            using (var exec = new DbExecutor(connection))
            {
                foreach (var item in exec.Select<T>(query, parameter, commandType))
                {
                    yield return item;
                }
            }
        }

        /// <summary>Executes and mapping objects by ColumnName - PropertyName.<para>When done dispose connection.</para></summary>
        /// <typeparam name="T">Mapping target Class.</typeparam>
        /// <param name="connection">Database connection.</param>
        /// <param name="query">SQL code.</param>
        /// <param name="parameter">PropertyName parameterized to PropertyName. if null then no use parameter.</param>
        /// <param name="commandType">Command Type.</param>
        /// <returns>Mapped instances.</returns>
        public static IEnumerable<T> Select<T>(IDbConnection connection, string query,
            object parameter = null, CommandType commandType = CommandType.Text)
            where T : new()
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(query));
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            return SelectHelper<T>(connection, query, parameter, commandType);
        }


        static IEnumerable<dynamic> SelectDynamicHelper(IDbConnection connection, string query, object parameter, CommandType commandType)
        {
            using (var exec = new DbExecutor(connection))
            {
                foreach (var item in exec.SelectDynamic(query, parameter, commandType))
                {
                    yield return item;
                }
            }
        }

        /// <summary>Executes and mapping objects to ExpandoObject. Object is dynamic accessable by ColumnName.<para>When done dispose connection.</para></summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="query">SQL code.</param>
        /// <param name="parameter">PropertyName parameterized to PropertyName. if null then no use parameter.</param>
        /// <param name="commandType">Command Type.</param>
        /// <returns>Mapped results(dynamic type is ExpandoObject).</returns>
        public static IEnumerable<dynamic> SelectDynamic(IDbConnection connection, string query,
            object parameter = null, CommandType commandType = CommandType.Text)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(query));
            Contract.Ensures(Contract.Result<IEnumerable<dynamic>>() != null);

            return SelectDynamicHelper(connection, query, parameter, commandType);
        }

        /// <summary>Insert by object's PropertyName.<para>When done dispose connection.</para></summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="tableName">Target database's table.</param>
        /// <param name="insertItem">Table's column name extracted from PropertyName.</param>
        /// <param name="parameterSymbol">Command parameter symbol. SqlServer = '@', MySql = '?', Oracle = ':'</param>
        /// <returns>Rows affected.</returns>
        public static int Insert(IDbConnection connection, string tableName, object insertItem, char parameterSymbol = '@')
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(tableName));
            Contract.Requires<ArgumentNullException>(insertItem != null);

            using (var exec = new DbExecutor(connection, parameterSymbol))
            {
                return exec.Insert(tableName, insertItem);
            }
        }

        /// <summary>Update by object's PropertyName.<para>When done dispose connection.</para></summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="tableName">Target database's table.</param>
        /// <param name="updateItem">Table's column name extracted from PropertyName.</param>
        /// <param name="whereCondition">Where condition extracted from PropertyName.</param>
        /// <param name="parameterSymbol">Command parameter symbol. SqlServer = '@', MySql = '?', Oracle = ':'</param>
        /// <returns>Rows affected.</returns>
        public static int Update(IDbConnection connection, string tableName, object updateItem, object whereCondition, char parameterSymbol = '@')
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(tableName));
            Contract.Requires<ArgumentNullException>(whereCondition != null);
            Contract.Requires<ArgumentNullException>(updateItem != null);

            using (var exec = new DbExecutor(connection, parameterSymbol))
            {
                return exec.Update(tableName, updateItem, whereCondition);
            }
        }

        /// <summary>Delete by object's PropertyName.<para>When done dispose connection.</para></summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="tableName">Target database's table.</param>
        /// <param name="whereCondition">Where condition extracted from PropertyName.</param>
        /// <param name="parameterSymbol">Command parameter symbol. SqlServer = '@', MySql = '?', Oracle = ':'</param>
        /// <returns>Rows affected.</returns>
        public static int Delete(IDbConnection connection, string tableName, object whereCondition, char parameterSymbol = '@')
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(tableName));
            Contract.Requires<ArgumentNullException>(whereCondition != null);

            using (var exec = new DbExecutor(connection, parameterSymbol))
            {
                return exec.Delete(tableName, whereCondition);
            }
        }
    }
}