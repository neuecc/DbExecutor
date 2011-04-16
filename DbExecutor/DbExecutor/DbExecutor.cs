using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace Codeplex.Data
{
    /// <summary>Simple and Lightweight Linq based Database Executor.</summary>
    public partial class DbExecutor : IDisposable
    {
        readonly IDbConnection connection;
        readonly IDbTransaction transaction;
        bool isTransactionCompleted = false;

        /// <summary>Connect start.</summary>
        /// <param name="connection">Database connection.</param>
        public DbExecutor(IDbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            Contract.EndContractBlock();

            this.connection = connection;
            if (connection.State != ConnectionState.Open) connection.Open();
            this.transaction = null;
        }

        /// <summary>Connect start and begin transaction.</summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="isolationLevel">Transaction IsolationLevel.</param>
        public DbExecutor(IDbConnection connection, IsolationLevel isolationLevel)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            Contract.EndContractBlock();

            this.connection = connection;
            if (connection.State != ConnectionState.Open) connection.Open();
            this.transaction = connection.BeginTransaction(isolationLevel);
        }

        private IDbCommand CreateCommand(string query, object parameter)
        {
            // Contract.Requires(!String.IsNullOrEmpty(query));

            var command = connection.CreateCommand();

            command.CommandText = query;
            if (parameter != null)
            {
                foreach (var p in PropertyCache.GetAccessors(parameter.GetType()))
                {
                    var param = command.CreateParameter();
                    param.ParameterName = "@" + p.Name;
                    param.Value = p.GetValue(parameter);
                    command.Parameters.Add(param);
                }
            }
            if (transaction != null) command.Transaction = transaction;

            return command;
        }

        private IDbCommand CreateCommand(string query, IEnumerable<IDataParameter> parameters)
        {
            // Contract.Requires(!String.IsNullOrEmpty(query));

            var command = connection.CreateCommand();

            command.CommandText = query;
            foreach (var p in parameters)
            {
                command.Parameters.Add(p);
            }
            if (transaction != null) command.Transaction = transaction;

            return command;
        }

        private IEnumerable<IDataRecord> ExecuteReader(IDbCommand command)
        {
            // Contract.Requires(command != null);

            using (command)
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read()) yield return reader;
            }
        }

        /// <summary>Executes and returns the data records."</summary>
        /// <param name="query">SQL code.</param>
        /// <param name="parameter">PropertyName parameterized to @PropertyName. if null then no use parameter.</param>
        /// <returns>Query results. This is lazy evaluation.</returns>
        public IEnumerable<IDataRecord> ExecuteReader(string query, object parameter = null)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException("query");
            Contract.Ensures(Contract.Result<IEnumerable<IDataRecord>>() != null);
            Contract.EndContractBlock();

            return ExecuteReader(CreateCommand(query, parameter));
        }

        /// <summary>Executes and returns the data records."</summary>
        /// <param name="query">SQL code.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns>Query results. This is lazy evaluation.</returns>
        public IEnumerable<IDataRecord> ExecuteReader(string query, IEnumerable<IDataParameter> parameters)
        {
            return ExecuteReader(CreateCommand(query, parameters));
        }

        /// <summary>Executes and returns the number of rows affected."</summary>
        public int ExecuteNonQuery(string query, object parameter = null)
        {
            Contract.Requires(!String.IsNullOrEmpty(query));

            using (var cmd = CreateCommand(query, parameter))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Executes and returns the number of rows affected."</summary>
        public int ExecuteNonQuery(string query, IEnumerable<IDataParameter> parameters)
        {
            using (var cmd = CreateCommand(query, parameters))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Executes and returns the first column.</summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="query">SQL code.</param>
        /// <param name="parameter">PropertyName parameterized to @PropertyName. if null then no use parameter.</param>
        /// <returns>Query results.</returns>
        public T ExecuteScalar<T>(string query, object parameter = null)
        {
            using (var cmd = CreateCommand(query, parameter))
            {
                return (T)cmd.ExecuteScalar();
            }
        }

        /// <summary>Executes and returns the first column.</summary>
        public T ExecuteScalar<T>(string query, IEnumerable<IDataParameter> parameters)
        {
            using (var cmd = CreateCommand(query, parameters))
            {
                return (T)cmd.ExecuteScalar();
            }
        }

        /// <summary>Executes and mapping objects by ColumnName - PropertyName."</summary>
        /// <typeparam name="T">Mapping target Class.</typeparam>
        /// <param name="query">SQL code.</param>
        /// <param name="parameter">PropertyName parameterized to @PropertyName. if null then no use parameter.</param>
        /// <returns>Query results. This is lazy evaluation.</returns>
        public IEnumerable<T> SelectTo<T>(string query, object parameter = null) where T : new()
        {
            var accessors = PropertyCache.GetAccessors(typeof(T));
            return ExecuteReader(query, parameter)
                .Select(dr =>
                {
                    var result = new T();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        if (dr.IsDBNull(i)) continue;

                        var accessor = accessors[dr.GetName(i)];
                        if (accessor != null) accessor.SetValue(result, dr[i]);
                    }
                    return result;
                });
        }

        /// <summary>Insert by object's PropertyName."</summary>
        /// <param name="tableName">Target database's table.</param>
        /// <param name="insertItem">Table's column name extracted from PropertyName.</param>
        public void InsertTo(string tableName, object insertItem)
        {
            var accessors = PropertyCache.GetAccessors(insertItem.GetType());
            var column = string.Join(", ", accessors.Select(p => p.Name));
            var data = string.Join(", ", accessors.Select(p => "@" + p.Name));

            var query = string.Format("insert into {0} ({1}) values ({2})", tableName, column, data);
            ExecuteNonQuery(query, insertItem);
        }

        /// <summary>Commit transaction.</summary>
        public void TransactionComplete()
        {
            if (transaction != null)
            {
                isTransactionCompleted = true;
                transaction.Commit();
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