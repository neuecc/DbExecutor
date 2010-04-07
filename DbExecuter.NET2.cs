/*--------------------------------------------------------------------------
* DbExecuter - linq based database executer
* ver 1.0.0.0 (Apr. 07th, 2010)
*
* created and maintained by neuecc <ils@neue.cc>
* licensed under Microsoft Public License(Ms-PL)
* http://neue.cc/
* http://dbexecuter.codeplex.com/
*--------------------------------------------------------------------------*/

// for Visual Studio 2005 or .NET Framework 2.0
// port from DbExecuter 1.0.0.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace Codeplex.Data
{
    /// <summary>Database Executer</summary>
    public class DbExecuter : IDisposable
    {
        private delegate TR Func<T1, T2, TR>(T1 t1, T2 t2);

        private readonly DbConnection dbConnection;

        public DbExecuter(DbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        private DbParameter CreateParameter(DbCommand cmd, string parameterName, object value)
        {
            DbParameter p = cmd.CreateParameter();
            p.ParameterName = parameterName;
            p.Value = value;
            return p;
        }

        private IEnumerable<DbCommand> UsingCommand(string query, object[] parameters)
        {
            using (DbCommand cmd = dbConnection.CreateCommand())
            {
                if (dbConnection.State != ConnectionState.Open) dbConnection.Open();
                cmd.CommandText = query;
                foreach (DbParameter p in Select<object, DbParameter>(parameters, delegate(object v, int i) { return CreateParameter(cmd, "@p" + i, v); }))
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
            return First(Select<DbCommand, T>(UsingCommand(query, parameters), delegate(DbCommand c) { return (T)c.ExecuteScalar(); }));
        }

        /// <summary>Executes and returns the number of rows affected."</summary>
        /// <param name="query">parameter name is applied "@p0, @p1,..."</param>
        public int ExecuteNonQuery(string query, params object[] parameters)
        {
            return First(Select<DbCommand, int>(UsingCommand(query, parameters), delegate(DbCommand c) { return c.ExecuteNonQuery(); }));
        }

        /// <summary>Executes and returns the data records."</summary>
        /// <param name="query">if parameters is blank then input null. parameter name is applied "@p0, @p1,..."</param>
        public List<T> ExecuteRead<T>(string query, object[] parameters, Converter<IDataRecord, T> selector)
        {
            if (parameters == null) parameters = new object[0];
            return new List<T>(Select<IDataRecord, T>(SelectMany<DbCommand, IDataRecord>(UsingCommand(query, parameters), delegate(DbCommand c) { return EnumerateAll(c); }), selector));
        }

        /// <summary>Executes and mapping objects by ColumnName - PropertyName."</summary>
        /// <param name="query">parameter name is applied "@p0, @p1,..."</param>
        public List<T> ExecuteQuery<T>(string query, params object[] parameters)
            where T : new()
        {
            return new List<T>(SelectMany<DbCommand, T>(UsingCommand(query, parameters), delegate(DbCommand c) { return Map<T>(c); }));
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
            using (DbExecuter executer = new DbExecuter(dbConnection))
            {
                return executer.ExecuteScalar<T>(query, parameters);
            }
        }

        /// <summary>Executes and returns the number of rows affected, when done dispose connection."</summary>
        /// <param name="query">parameter name is applied "@p0, @p1,..."</param>
        public static int ExecuteNonQuery(DbConnection dbConnection, string query, params object[] parameters)
        {
            using (DbExecuter executer = new DbExecuter(dbConnection))
            {
                return executer.ExecuteNonQuery(query, parameters);
            }
        }

        /// <summary>Executes and returns the data records, when done dispose connection."</summary>
        /// <param name="query">if parameters is blank then input null. parameter name is applied "@p0, @p1,..."</param>
        public static List<T> ExecuteRead<T>(DbConnection dbConnection, string query, object[] parameters, Converter<IDataRecord, T> selector)
        {
            using (DbExecuter executer = new DbExecuter(dbConnection))
            {
                return executer.ExecuteRead(query, parameters, selector);
            }
        }

        /// <summary>Executes and mapping object by ColumnName - PropertyName, when done dispose connection."</summary>
        /// <param name="query">parameter name is applied "@p0, @p1,..."</param>
        public static List<T> ExecuteQuery<T>(DbConnection dbConnection, string query, params object[] parameters)
            where T : new()
        {
            using (DbExecuter executer = new DbExecuter(dbConnection))
            {
                return executer.ExecuteQuery<T>(query, parameters);
            }
        }

        #endregion

        #region DbCommandExtensions

        /// <summary>Enumerate Rows</summary>
        private static IEnumerable<IDataRecord> EnumerateAll(IDbCommand command)
        {
            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read()) yield return reader;
            }
        }

        /// <summary>ColumnName - PropertyName Mapping</summary>
        private static IEnumerable<T> Map<T>(IDbCommand command) where T : new()
        {
            return DbMapper<T>.Map(command);
        }

        // Cache for Map
        private static class DbMapper<T> where T : new()
        {
            static readonly Dictionary<string, PropertyInfo> propertyCache;

            static DbMapper()
            {
                propertyCache = new Dictionary<string, PropertyInfo>();
                foreach (PropertyInfo pi in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    propertyCache.Add(pi.Name, pi);
                }
            }

            public static IEnumerable<T> Map(IDbCommand command)
            {
                return Select<IDataRecord, T>(EnumerateAll(command), delegate(IDataRecord dr)
                {
                    T result = new T();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        if (dr.IsDBNull(i)) continue;
                        propertyCache[dr.GetName(i)].SetValue(result, dr[i], null);
                    }
                    return result;
                });
            }
        }

        #endregion

        #region Linq to Objects port

        private static IEnumerable<TR> Select<T, TR>(IEnumerable<T> source, Converter<T, TR> selector)
        {
            foreach (T item in source)
            {
                yield return selector(item);
            }
        }

        private static IEnumerable<TR> Select<T, TR>(IEnumerable<T> source, Func<T, int, TR> selector)
        {
            int index = 0;
            foreach (T item in source)
            {
                yield return selector(item, index++);
            }
        }

        private static IEnumerable<TR> SelectMany<T, TR>(IEnumerable<T> source, Converter<T, IEnumerable<TR>> selector)
        {
            foreach (T item in source)
            {
                foreach (TR subItem in selector(item))
                {
                    yield return subItem;
                }
            }
        }

        private static T First<T>(IEnumerable<T> source)
        {
            foreach (T item in source)
            {
                return item;
            }
            throw new InvalidOperationException();
        }

        #endregion
    }
}