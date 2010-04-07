/*--------------------------------------------------------------------------
* DbExecuter - linq based database executer
* ver 1.0.0.0 (Apr. 07th, 2010)
*
* created and maintained by neuecc <ils@neue.cc>
* licensed under Microsoft Public License(Ms-PL)
* http://neue.cc/
* http://dbexecuter.codeplex.com/
*--------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Codeplex.Data.Extensions;

namespace Codeplex.Data
{
    /// <summary>linq based database executer</summary>
    public class DbExecuter : IDisposable
    {
        private readonly DbConnection dbConnection;

        public DbExecuter(DbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        private DbParameter CreateParameter(DbCommand cmd, string parameterName, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = parameterName;
            p.Value = value;
            return p;
        }

        private IEnumerable<DbCommand> UsingCommand(string query, object[] parameters)
        {
            using (var cmd = dbConnection.CreateCommand())
            {
                if (dbConnection.State != ConnectionState.Open) dbConnection.Open();
                cmd.CommandText = query;
                foreach (var p in parameters.Select((v, i) => CreateParameter(cmd, "@p" + i, v)))
                {
                    cmd.Parameters.Add(p);
                }
                yield return cmd;
            }
        }

        /// <summary>Executes and returns the first column.</summary>
        /// <param name="query">parameter name is applied "@p0, @p1,..."</param>
        public T ExecuteScalar<T>(string query, params object[] parameters)
        {
            return UsingCommand(query, parameters).Select(c => (T)c.ExecuteScalar()).First();
        }

        /// <summary>Executes and returns the number of rows affected."</summary>
        /// <param name="query">parameter name is applied "@p0, @p1,..."</param>
        public int ExecuteNonQuery(string query, params object[] parameters)
        {
            return UsingCommand(query, parameters).Select(c => c.ExecuteNonQuery()).First();
        }

        /// <summary>Executes and returns the data records."</summary>
        /// <param name="query">parameter name is applied "@p0, @p1,..."</param>
        public IEnumerable<IDataRecord> ExecuteRead(string query, params object[] parameters)
        {
            return UsingCommand(query, parameters).SelectMany(c => c.EnumerateAll());
        }

        /// <summary>Executes and mapping objects by ColumnName - PropertyName."</summary>
        /// <param name="query">parameter name is applied "@p0, @p1,..."</param>
        public IEnumerable<T> ExecuteQuery<T>(string query, params object[] parameters)
            where T : new()
        {
            return UsingCommand(query, parameters).SelectMany(c => c.Map<T>());
        }

        /// <summary>dispose inner connection</summary>
        public void Dispose()
        {
            dbConnection.Dispose();
        }

        #region Static Methods

        /// <summary>Executes and returns the first column, when done dispose connection."</summary>
        /// <param name="query">parameter name is applied "@p0, @p1,..."</param>
        public static T ExecuteScalar<T>(DbConnection dbConnection, string query, params object[] parameters)
        {
            using (var executer = new DbExecuter(dbConnection))
            {
                return executer.ExecuteScalar<T>(query, parameters);
            }
        }

        /// <summary>Executes and returns the number of rows affected, when done dispose connection."</summary>
        /// <param name="query">parameter name is applied "@p0, @p1,..."</param>
        public static int ExecuteNonQuery(DbConnection dbConnection, string query, params object[] parameters)
        {
            using (var executer = new DbExecuter(dbConnection))
            {
                return executer.ExecuteNonQuery(query, parameters);
            }
        }

        /// <summary>Executes and returns the data records, when done dispose connection."</summary>
        /// <param name="query">parameter name is applied "@p0, @p1,..."</param>
        public static IEnumerable<IDataRecord> ExecuteRead(DbConnection dbConnection, string query, params object[] parameters)
        {
            using (var executer = new DbExecuter(dbConnection))
            {
                foreach (var item in executer.ExecuteRead(query, parameters)) yield return item;
            }
        }

        /// <summary>Executes and mapping object by ColumnName - PropertyName, when done dispose connection."</summary>
        /// <param name="query">parameter name is applied "@p0, @p1,..."</param>
        public static IEnumerable<T> ExecuteQuery<T>(DbConnection dbConnection, string query, params object[] parameters)
            where T : new()
        {
            using (var executer = new DbExecuter(dbConnection))
            {
                foreach (var item in executer.ExecuteQuery<T>(query, parameters)) yield return item;
            }
        }

        #endregion
    }
}

namespace Codeplex.Data.Extensions
{
    public static class IDbCommandExtensions
    {
        /// <summary>Enumerate Rows</summary>
        public static IEnumerable<IDataRecord> EnumerateAll(this IDbCommand command)
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read()) yield return reader;
            }
        }

        /// <summary>ColumnName - PropertyName Mapping</summary>
        public static IEnumerable<T> Map<T>(this IDbCommand command) where T : new()
        {
            return DbMapper<T>.Map(command);
        }

        // Cache for Map
        private static class DbMapper<T> where T : new()
        {
            static readonly Dictionary<string, PropertyInfo> propertyCache;

            static DbMapper()
            {
                propertyCache = typeof(T)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .ToDictionary(pi => pi.Name);
            }

            public static IEnumerable<T> Map(IDbCommand command)
            {
                return command.EnumerateAll().Select(dr =>
                {
                    var result = new T();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        if (dr.IsDBNull(i)) continue;
                        propertyCache[dr.GetName(i)].SetValue(result, dr[i], null);
                    }
                    return result;
                });
            }
        }
    }
}