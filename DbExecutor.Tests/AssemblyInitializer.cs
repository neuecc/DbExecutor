using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Codeplex.Data;

namespace DbExecutorTest
{
    // const

    public static class Db
    {
        public const string ConnectionString = @"Data Source=.;Initial Catalog=tempdb;Integrated Security=True";

        public static readonly Func<SqlConnection> ConnectionFactory =
            () => new SqlConnection(ConnectionString);
    }

    // codefirst model

    public class Type
    {
        public int TypeId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Method> Properties { get; set; }

        public override string ToString()
        {
            return string.Format(@"{{TypeId = {0}, Name = {1}}}", TypeId, Name);
        }
    }

    public class Method
    {
        public int TypeId { get; set; }
        public int MethodId { get; set; }
        public string Name { get; set; }

        public virtual Type Type { get; set; }
    }

    public class CSharpStructure : DbContext
    {
        public DbSet<Type> Types { get; set; }
        public DbSet<Method> Methods { get; set; }
    }

    // initializer

    [TestClass]
    public class AssemblyInitializer
    {
        [AssemblyInitialize]
        public static void Init(TestContext tc)
        {
            Contract.ContractFailed += (sender, e) =>
            {
                e.SetUnwind();
                Assert.Fail(e.FailureKind.ToString() + ":" + e.Message);
            };

            InitCompactDb();
            InitServerDb();
        }

        static void InitCompactDb()
        {
            Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");
            Database.SetInitializer(new DropCreateDatabaseAlways<CSharpStructure>());

            using (var cx = new CSharpStructure())
            {
                var types = new[]
                {
                    new Type{Name = "Int32"},
                    new Type{Name = "String"},
                    new Type{Name = "ListOfT"},
                    new Type{Name = "DictionaryOfTOfT"},
                };
                foreach (var item in types) cx.Types.Add(item);

                var methods = new[]
                {
                    new Method{Name = "CompareTo", Type = types[0]},
                    new Method{Name = "StartsWith", Type= types[1]},
                    new Method{Name = "EndsWith", Type= types[1]},
                    new Method{Name = "Contains", Type= types[1]},
                    new Method{Name = "TrueForAll", Type= types[2]},
                    new Method{Name = "ForEach", Type= types[2]},
                    new Method{Name = "ContainsKey", Type= types[3]},
                    new Method{Name = "TryGetValue", Type= types[3]},
                };
                foreach (var item in methods) cx.Methods.Add(item);

                cx.SaveChanges();
            }
        }

        static void InitServerDb()
        {
            using (var exec = new DbExecutor(Db.ConnectionFactory()))
            {
                exec.ExecuteNonQuery(@"
if (OBJECT_ID('Type') is null)
begin
    create table Type
    (
        TypeId int identity primary key,
        Name nvarchar(max)
    )
    create table Method
    (
        MethodId int identity primary key,
        TypeId int,
        Name nvarchar(max)
    )
end
                    ");

                var types = new[]
                {
                    new Type{Name = "Int32", TypeId = 1},
                    new Type{Name = "String", TypeId = 2},
                    new Type{Name = "ListOfT", TypeId = 3},
                    new Type{Name = "DictionaryOfTOfT", TypeId = 4},
                };
                foreach (var item in types)
                {
                    exec.Insert("Type", new { item.Name });
                }

                var methods = new[]
                {
                    new Method{Name = "CompareTo", Type = types[0]},
                    new Method{Name = "StartsWith", Type= types[1]},
                    new Method{Name = "EndsWith", Type= types[1]},
                    new Method{Name = "Contains", Type= types[1]},
                    new Method{Name = "TrueForAll", Type= types[2]},
                    new Method{Name = "ForEach", Type= types[2]},
                    new Method{Name = "ContainsKey", Type= types[3]},
                    new Method{Name = "TryGetValue", Type= types[3]},
                };
                foreach (var item in methods)
                {
                    exec.Insert("Method", new { item.Name, item.Type.TypeId });
                }
            }
        }

        [AssemblyCleanup]
        public static void CleanServerDb()
        {
            using (var exec = new DbExecutor(Db.ConnectionFactory()))
            {
                exec.ExecuteNonQuery(@"
                    drop table Type
                    drop table Method");
            }
        }
    }
}
