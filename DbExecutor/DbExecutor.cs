using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Diagnostics.Contracts;
using Codeplex.Data.Infrastructure;

namespace Codeplex.Data
{
    /// <summary>Simple and Lightweight Linq based Database Executor.</summary>
    public partial class DbExecutor : IDisposable
    {
        readonly IDbConnection connection;
        // Transaction
        readonly bool isUseTransaction;
        readonly IsolationLevel isolationLevel;
        IDbTransaction transaction;
        bool isTransactionCompleted = false;

        /// <summary>Connect start.</summary>
        /// <param name="connection">Database connection.</param>
        public DbExecutor(IDbConnection connection)
        {
            Contract.Requires(connection != null);

            this.connection = connection;
            this.isUseTransaction = false;
        }

        /// <summary>Use ransaction.</summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="isolationLevel">Transaction IsolationLevel.</param>
        public DbExecutor(IDbConnection connection, IsolationLevel isolationLevel)
        {
            Contract.Requires(connection != null);

            this.connection = connection;
            this.isUseTransaction = true;
            this.isolationLevel = isolationLevel;
        }

        /// <summary>If connection is not open then open and create command.</summary>
        private IDbCommand PrepareExecute(string query, CommandType commandType, params object[] parameters)
        {
            Contract.Requires(!String.IsNullOrEmpty(query));
            Contract.Ensures(Contract.Result<IDbCommand>() != null);

            if (connection.State != ConnectionState.Open) connection.Open();
            if (transaction == null && isUseTransaction) transaction = connection.BeginTransaction(isolationLevel);

            var command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = commandType;

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter == null) continue;

                foreach (var p in PropertyCache.Lookup(parameter.GetType()))
                {
                    if (!p.IsReadable) continue;

                    Contract.Assume(parameter != null);
                    var param = command.CreateParameter();
                    param.ParameterName = p.Name;
                    param.Value = p.GetValue(ref parameter);
                    command.Parameters.Add(param);
                }
            }
            if (transaction != null) command.Transaction = transaction;

            return command;
        }

        public IEnumerable<IDataRecord> ExecuteReader(string query, object parameter = null)
        {
            Contract.Requires(!String.IsNullOrEmpty(query));
            Contract.Ensures(Contract.Result<IEnumerable<IDataRecord>>() != null);

            return ExecuteReader(query, CommandType.Text, parameter);
        }

        /// <summary>Executes and returns the data records."</summary>
        /// <param name="query">SQL code.</param>
        /// <param name="commandType">Command type.</param>
        /// <param name="parameter">PropertyName parameterized to @PropertyName. if null then no use parameter.</param>
        /// <returns>Query results. This is lazy evaluation.</returns>
        public IEnumerable<IDataRecord> ExecuteReader(string query, CommandType commandType, object parameter = null)
        {
            Contract.Requires(!String.IsNullOrEmpty(query));
            Contract.Ensures(Contract.Result<IEnumerable<IDataRecord>>() != null);

            using (var command = PrepareExecute(query, commandType, parameter))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read()) yield return reader;
            }

        }

        /// <summary>Executes and returns the number of rows affected."</summary>
        public int ExecuteNonQuery(string query, object parameter = null)
        {
            Contract.Requires(!String.IsNullOrEmpty(query));

            return ExecuteNonQuery(query, CommandType.Text, parameter);
        }

        public int ExecuteNonQuery(string query, CommandType commandType, object parameter = null)
        {
            Contract.Requires(!String.IsNullOrEmpty(query));

            using (var command = PrepareExecute(query, commandType, parameter))
            {
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>Executes and returns the first column.</summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="query">SQL code.</param>
        /// <param name="parameter">PropertyName parameterized to @PropertyName. if null then no use parameter.</param>
        /// <returns>Query results.</returns>
        public T ExecuteScalar<T>(string query, object parameter = null)
        {
            Contract.Requires(!String.IsNullOrEmpty(query));
            Contract.Ensures(Contract.Result<T>() != null);

            return ExecuteScalar<T>(query, CommandType.Text, parameter);
        }

        /// <summary>Executes and returns the first column.</summary>
        public T ExecuteScalar<T>(string query, CommandType commandType, object parameter = null)
        {
            Contract.Requires(!String.IsNullOrEmpty(query));
            Contract.Ensures(Contract.Result<T>() != null);

            using (var command = PrepareExecute(query, commandType, parameter))
            {
                var result = (T)command.ExecuteScalar();

                Contract.Assume(result != null);
                return result;
            }
        }

        /// <summary>Executes and mapping objects by ColumnName - PropertyName."</summary>
        /// <typeparam name="T">Mapping target Class.</typeparam>
        /// <param name="query">SQL code.</param>
        /// <param name="parameter">PropertyName parameterized to @PropertyName. if null then no use parameter.</param>
        /// <returns>Query results. This is lazy evaluation.</returns>
        public IEnumerable<T> Select<T>(string query, object parameter = null) where T : new()
        {
            Contract.Requires(!String.IsNullOrEmpty(query));
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            return Select<T>(query, CommandType.Text, parameter);
        }

        public IEnumerable<T> Select<T>(string query, CommandType commandType, object parameter = null) where T : new()
        {
            Contract.Requires(!String.IsNullOrEmpty(query));
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            var accessors = PropertyCache.Lookup(typeof(T));
            return ExecuteReader(query, parameter)
                .Select(dr =>
                {
                    object result = new T();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        if (dr.IsDBNull(i)) continue;

                        var accessor = accessors[dr.GetName(i)];
                        if (accessor != null && accessor.IsWritable) accessor.SetValue(ref result, dr[i]);
                    }
                    return (T)result;
                });
        }

        /// <summary>Insert by object's PropertyName."</summary>
        /// <param name="tableName">Target database's table.</param>
        /// <param name="insertItem">Table's column name extracted from PropertyName.</param>
        public int Insert(string tableName, object insertItem)
        {
            Contract.Requires(!String.IsNullOrEmpty(tableName));
            Contract.Requires(insertItem != null);

            var propNames = PropertyCache.Lookup(insertItem.GetType())
                .Where(p => p.IsReadable)
                .ToArray();
            var column = string.Join(", ", propNames.Select(p => p.Name));
            var data = string.Join(", ", propNames.Select(p => "@" + p.Name));

            var query = string.Format("insert into {0} ({1}) values ({2})", tableName, column, data);

            Contract.Assume(query.Length > 0);
            return ExecuteNonQuery(query, insertItem);
        }

        public int Update(string tableName, object whereCondition, object updateParameter)
        {
            Contract.Requires(!String.IsNullOrEmpty(tableName));
            Contract.Requires(whereCondition != null);
            Contract.Requires(updateParameter != null);

            var update = string.Join(", ", PropertyCache.Lookup(updateParameter.GetType())
                .Where(p => p.IsReadable)
                .Select(p => p.Name + " = " + "@" + p.Name));

            var where = string.Join(" and ", PropertyCache.Lookup(whereCondition.GetType())
                .Select(p => p.Name + " = " + p.GetValue(ref whereCondition)));

            var query = string.Format("update {0} set {1} where {2}", tableName, update, where);

            Contract.Assume(query.Length > 0);
            using (var command = PrepareExecute(query, CommandType.Text, whereCondition, updateParameter))
            {
                return command.ExecuteNonQuery();
            }
        }

        public int Delete(string tableName, object whereCondition)
        {
            Contract.Requires(!String.IsNullOrEmpty(tableName));
            Contract.Requires(whereCondition != null);

            var where = string.Join(" and ", PropertyCache.Lookup(whereCondition.GetType())
                .Select(p => p.Name + " = " + p.GetValue(ref whereCondition)));

            var query = string.Format("delete from {0} where {1}", tableName, where);

            Contract.Assume(query.Length > 0);
            return ExecuteNonQuery(query, whereCondition);
        }

        /// <summary>Commit transaction.</summary>
        public void TransactionComplete()
        {
            if (transaction != null)
            {
                transaction.Commit();
                isTransactionCompleted = true;
            }
        }

        /// <summary>Dispose inner connection.</summary>
        public void Dispose()
        {
            if (transaction != null && !isTransactionCompleted)
            {
                transaction.Rollback();
            }
            connection.Dispose();
        }
    }
}